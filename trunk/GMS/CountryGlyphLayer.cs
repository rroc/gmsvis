using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;

using Gav.Data;
using Gav.Graphics;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Windows.Forms;

namespace GMS
{
    public class CountryGlyphLayer : MapGlyphLayer
    {
        // Glyphs
        private List<Texture> iTexture;
        private const int GLYPH_AGE = 1;
        private const int GLYPH_ALBUMS = 2;
        private const int GLYPH_WORK   = 3;
        private const int GLYPH_MONEY = 4;
        
        private CustomVertex.PositionTextured[] iBasicGlyph;

        private bool _inited;

        private IDataCubeProvider<float> _input;

        private List<AxisMap> _axisMaps;
        private Panel iPanel;


        public IDataCubeProvider<float> Input
        {
            get { return _input; }
            set { _input = value; }
        }


        public CountryGlyphLayer(Panel aPanel) 
        {
            iPanel = aPanel;
        }


        //----------------------------------------------------------

        private void InitMapGlyphs()
        {
            _device.RenderState.Lighting = false;       

            string dir = Directory.GetCurrentDirectory();
            string dataPath = "\\..\\..\\..\\data\\glyphs\\";

            iTexture = new List<Texture>();
            iTexture.Insert(GLYPH_AGE    - 1, TextureLoader.FromFile(_device, dir + dataPath + "age.png"));
            iTexture.Insert(GLYPH_ALBUMS - 1, TextureLoader.FromFile(_device, dir + dataPath + "cd.png"));
            iTexture.Insert(GLYPH_WORK   - 1, TextureLoader.FromFile(_device, dir + dataPath + "work.png"));
            iTexture.Insert(GLYPH_MONEY  - 1, TextureLoader.FromFile(_device, dir + dataPath + "coin.png"));

            float scale = 10;
            iBasicGlyph = new CustomVertex.PositionTextured[6];
            iBasicGlyph[0].Position = new Vector3(scale * -1.5F, scale * -1.5F, 0);
            iBasicGlyph[1].Position = new Vector3(scale *  1.5F, scale * -1.5F, 0);
            iBasicGlyph[2].Position = new Vector3(scale * -1.5F, scale *  1.5F, 0);

            iBasicGlyph[3].Position = new Vector3(scale * -1.5F, scale *  1.5F, 0);
            iBasicGlyph[4].Position = new Vector3(scale *  1.5F, scale * -1.5F, 0);
            iBasicGlyph[5].Position = new Vector3(scale *  1.5F, scale *  1.5F, 0);

            //Texture coordinates
            iBasicGlyph[0].Tu = 0;
            iBasicGlyph[0].Tv = 1;

            iBasicGlyph[1].Tu = 1;
            iBasicGlyph[1].Tv = 1;

            iBasicGlyph[2].Tu = 0;
            iBasicGlyph[2].Tv = 0;

            iBasicGlyph[3].Tu = 0;
            iBasicGlyph[3].Tv = 0;

            iBasicGlyph[4].Tu = 1;
            iBasicGlyph[4].Tv = 1;

            iBasicGlyph[5].Tu = 1;
            iBasicGlyph[5].Tv = 0;
        }

        private void DrawAllGlyphs(int aItem )
        {
            _device.RenderState.Lighting = false;
            _device.VertexFormat = CustomVertex.PositionTextured.Format;

            //Save states
            Matrix oldTransform = _device.Transform.World;

            CheckedListBox a = (CheckedListBox)(iPanel.Controls[0].Controls[0]);

            int j = 0;
            for (int i = 1, iEnd = iTexture.Count + 1; i < iEnd; i++) 
            {
                //Restore Previous values
                _device.Transform.World = oldTransform;

                //if glypf is selected
                if (a.CheckedIndices.Contains(i - 1))
                {
                    //Translate nicely
                    if (0 == j)
                        _device.Transform.World *= Matrix.Translation(0.0F, 10.0F, 0);
                    else if (1 == j)
                        _device.Transform.World *= Matrix.Translation(-14.0F, 0.0F, 0);
                    else if (2 == j)
                        _device.Transform.World *= Matrix.Translation(14.0F, 0.0F, 0);
                    else if (3 == j)
                        _device.Transform.World *= Matrix.Translation(0.0F, -10.0F, 0);

                    //draw the item
                    DrawGlyph(i, aItem);
                    j++;
                }
            }

            //Restore Previous values
            _device.Transform.World = oldTransform;

            //Restore Previous values
            _device.Transform.World = oldTransform;

            //restore texture changes
            _device.VertexFormat = CustomVertex.PositionColored.Format;
            _device.SetTexture(0, null);
        }

        private void DrawGlyph(int aIndex, int aItem )
        {
            int maxValue = 9; //(max-1)
            _device.SetTexture(0, iTexture[aIndex -1]);

            float count = _axisMaps[aIndex].MappedValues[aItem];

            //EXCEPTION
            if (GLYPH_WORK == aIndex) 
            {
                count = 1.0F - count;
            }

            int maxAmount = 1 + (int)(maxValue * count);

            for (int i = 0; i < maxAmount; i++)
            {
                //Translate
                _device.Transform.World *= Matrix.Translation(0.0F, 3.5F, 0);
                _device.DrawUserPrimitives(PrimitiveType.TriangleList, 2, iBasicGlyph);
            }
        }



        // Creates one axismap per column in the data set. 
        private void CreateAxisMaps()
        {
            _axisMaps = new List<AxisMap>();

            for (int i = 0; i < _input.GetData().GetAxisLength(Axis.X); i++)
            {
                AxisMap axisMap = new AxisMap();
                axisMap.Input = _input;
                axisMap.Index = i;
                axisMap.DoMapping();
                _axisMaps.Add(axisMap);
            }
        }

        // This method is called every time the map is rendered. 
        protected override void InternalRender()
        {

            // If the input is null we cannot render.
            if (_input == null)
            {
                return;
            }

            // If the glyph is not inited, call InternalInit. 
            if (!_inited)
            {
                InternalInit(_device);
                if (!_inited)
                {
                    return;
                }
            }

            _device.RenderState.CullMode = Cull.None;

            // Loops through the regions in the map.
            for (int i = 0; i < _input.GetData().GetAxisLength(Axis.Y); i++)
            {
                // Resets the world transform.  
                _device.Transform.World = _layerWorldMatrix;

                // If a glyph positioner (a class that moves the glyphs to the correct position) is set, use it. 
                if (ActiveGlyphPositioner != null)
                {
                    //Gets the position for the glyph with index i.
                    Vector2 pos = ActiveGlyphPositioner.GetPosition(i);
                    // Translates the world transform. 
                    _device.Transform.World *= Matrix.Translation(
                        pos.X,
                        pos.Y,
                        0
                        );
                }

                if (IndexVisibilityHandler.GetVisibility(i))
                {
                    DrawAllGlyphs(i);
                }
            }
        }

        // This method is called once when the glyph is inited. 
        protected override void InternalInit(Device device)
        {

            if (_input == null)
            {
                return;
            }
            InitMapGlyphs();
            CreateAxisMaps();
            _inited = true;
        }

        protected override void InternalInvalidate() { }
    }
}