using System;
using System.Collections.Generic;
using System.Text;
using Gav.Data;



namespace GMS
{
    class KMeansFilter : Filter<float, float> {

        private List<List<int>> clusters;
        private List<List<float>> clusterMeans;
        private int nClusters;
        //private bool randomClustersReady;

        public KMeansFilter(int aNClusters)
        {
            clusters = new List<List<int>>();
            clusterMeans = new List<List<float>>();
            for (int i = 0; i < aNClusters; i++)
            {
                clusters.Add( new List<int>() );
                clusterMeans.Add(new List<float>());
            }

            nClusters = aNClusters;
            //randomClustersReady = false;
        }

        private void GenerateRandomList(int totalElements)
        {
            int samplesPerCluster = (int)Math.Ceiling((double)totalElements / (double)nClusters);
            Random randomGen = new Random();
            List<int> bag = new List<int>();

            for (int i = 0; i < totalElements; i++)
            {
                bag.Add(i);
            }

            int currentList = 0;
            while( bag.Count != 0 )
            {
                int rand = randomGen.Next(bag.Count);
                int row = bag[rand];
                bag.Remove(row);
               
                clusters[currentList].Add( row );

                if ( clusters[currentList].Count >= samplesPerCluster )
                {
                    currentList++;
                }
            }

            //randomClustersReady = true;
        }

        private void NormalizeData(float[, ,] normalizedData)
        {
            List<float> max, min;
            _input.GetData().GetAllColumnMaxMin(out max, out min);
            int sizeY = _input.GetData().Data.GetLength(1);
            int sizeX = _input.GetData().Data.GetLength(0);

            for (int i = 0; i < sizeX; i++)
            {
                // iterate through the columns
                for (int j = 0; j < sizeY; j++)
                {
                    normalizedData[i, j, 0] = (_input.GetData().Data[i, j, 0] - min[i]) / (max[i] - min[i]);
                }
            }
        }

        private int GetClosestCluster(List<List<int>> allClusters, 
            int rowNumber, float[, ,] rows)
        {
            float min = float.MaxValue;
            int closestCluster = 0;
            for (int i = 0; i < allClusters.Count; i++ )
            {
                float distance = DistanceItemMean(clusterMeans[i], rowNumber, rows);
                if ( distance < min)
                {
                    min = distance;
                    closestCluster = i;
                }
            }

            return closestCluster;
        }

        private float DistanceItemMean(List<float> mean, int rowNumber, float[, ,] rows)
        {
            float sum = 0.0f;
            int col = 0;
            foreach (float value in mean)
            {
                float columnValue = rows[col++, rowNumber, 0];
                float element = value - columnValue;
                sum += element * element;
            }

            return sum;
        }

        private bool RecomputeClusters(float[, ,] data)
        {
            bool changed = false;
            int currentCluster = 0;
            foreach (List<int> cluster in clusters)
            {
                for (int i = 0; i < cluster.Count; i++)
                {
                    int row = cluster[i];
                    int closestCluster = GetClosestCluster(clusters, row, data);
                    if ( closestCluster != currentCluster && 1<cluster.Count)
                    {
                        // move the row from current to closest
                        cluster.Remove( row );
                        clusters[closestCluster].Add(row);
                        i--;

                        changed = true;
                    }
                    
                }
                currentCluster++;
            }

            return changed;
        }

        private void GenerateMeans(float[, ,] normalizedData, int rowCount, int columnCount)
        {
            // clear the list from previous computed means
            foreach (List<float> cluster in clusterMeans)
            {
                cluster.Clear();
            }

            int clusterCount = 0;

            // Generate first means
            foreach (List<int> cluster in clusters)
            {
                for (int i = 0; i < columnCount; i++)
                {
                    float columnMean = GetColumnMean(cluster, normalizedData, i);

                    // for the current cluster, add the mean for that column
                    clusterMeans[clusterCount].Add(columnMean);
                }
                clusterCount++;
            }
        }

        protected override void ProcessData() 
        {

            // The filter's input data.
            float[, ,] inputData = _input.GetData().Data;

            int columnCount = _input.GetData().Data.GetLength(0);
            int rowCount = _input.GetData().Data.GetLength(1);

            // Implement the KMeans clustering algorithm here. You can of course split up your code into several methods.
            //if( ! randomClustersReady )
            {
                float[, ,] normalizedData = new float[columnCount, rowCount, 1];
                NormalizeData(normalizedData);

                GenerateRandomList(rowCount);

                GenerateMeans(normalizedData, rowCount, columnCount);

                // Compute Distances and Move according to the smallest distance
                while ( RecomputeClusters( normalizedData ) )
                {
                    GenerateMeans(normalizedData, rowCount, columnCount);
                }

                // The output data should be set to the _dataCube.Data array.
                float[, ,] output = new float[columnCount + 1, rowCount, 1];

                for (int i = 0; i < rowCount; i++)
                {
                    for (int j = 0; j < columnCount; j++)
                    {
                        output[j, i, 0] = inputData[j, i, 0];
                    }
                }

                CopyClusterData(output, columnCount);

                for (int i = 0; i < nClusters; i++ )
                {
                    clusters[i].Clear();
                    clusterMeans[i].Clear();
                }


                _dataCube.Data = output;
            }

        }

        //////////////////////////////////////////////////////////////////////////
        // Method:    CopyClusterData
        // FullName:  GMS.KMeansFilter.CopyClusterData
        // Access:    private 
        // Returns:   void
        // Parameter: float[, ,] output
        // Parameter: int columnCount
        //////////////////////////////////////////////////////////////////////////
        private void CopyClusterData(float[, ,] output, int columnCount)
        {
            for(int i = 0; i < clusters.Count; i++)
            {
                List<int> cluster = clusters[ i ];

                for(int j = 0; j < cluster.Count; j++)
                {
                    int row = cluster[ j ];
                    output[columnCount, row, 0] = i;
                }
            }
        }

        //////////////////////////////////////////////////////////////////////////
        // Method:    GetColumnMean
        // FullName:  GMS.KMeansFilter.GetColumnMean
        // Access:    private 
        // Returns:   float
        // Parameter: List<int> cluster
        // Parameter: float[, ,] inputData
        // Parameter: int column
        //////////////////////////////////////////////////////////////////////////
        private float GetColumnMean(List<int> cluster, float[, ,] inputData, int column)
        {
            float sum = 0.0f;
            foreach (int row in cluster)
            {
                float value = inputData[column, row, 0];
                sum += value;
            }

            return sum / cluster.Count;

            //int len = inputData.GetLength( 1 );
            //float sum = 0.0f;
            //for (int i = 0; i < len; i++)
            //{
            //    sum += inputData[column, i, 0];
            //}

            //return sum / len;
        }

        //////////////////////////////////////////////////////////////////////////
        // Method:    SetNumberOfClusters
        // FullName:  GMS.KMeansFilter.SetNumberOfClusters
        // Access:    public 
        // Returns:   void
        // Parameter: int aClusters
        //////////////////////////////////////////////////////////////////////////
        public void SetNumberOfClusters(int aClusters)
        {
            int previousNClusters = nClusters;
            nClusters = aClusters;

            int diff = nClusters - previousNClusters;

            // if there are any differences in sizes: remove or add
            if( diff > 0)
            {
                for (int i = 0; i < diff; i++)
                {
                    clusters.Add(new List<int>());
                    clusterMeans.Add(new List<float>());
                }
            }
            else
            {
                for (int i = 0; i < diff; i++)
                {
                    clusters.RemoveAt(clusters.Count - 1);
                    clusterMeans.RemoveAt(clusterMeans.Count - 1);
                }
            }
        }
    }
}
