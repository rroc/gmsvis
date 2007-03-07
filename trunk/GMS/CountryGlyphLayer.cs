using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;

using Gav.Data;
using Gav.Graphics;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace GMS
{
    public class CountryGlyphLayer : MapGlyphLayer
    {
        // Glyphs
        private Texture iTexture;
        private Material iMaterial;
        private CustomVertex.PositionTextured[] iGlyphMoney;

        private bool _inited;
        private List<AxisMap> _axisMaps;

        private IDataCubeProvider<float> _input;

        private ColorMap colorMap;

        public IDataCubeProvider<float> Input
        {
            get { return _input; }
            set { _input = value; }
        }

        //private IndexVisibilityHandler _visibilityHandler;

        //public IndexVisibilityHandler IndexVisibilityHandler
        //{
        //    get { return _visibilityHandler; }
        //    set { _visibilityHandler = value; }
        //}


        //----------------------------------------------------------

        private void InitMapGlyphs()
        {
            iMaterial = new Material();

            iMaterial.Diffuse = Color.White;
            iMaterial.Specular = Color.LightGray;
            iMaterial.SpecularSharpness = 15.0F;

            _device.Material = iMaterial;

            string dir = Directory.GetCurrentDirectory();
            string dataPath = "\\..\\..\\..\\data\\glyphs\\";

            iTexture = TextureLoader.FromFile(_device, dir+dataPath+"money.jpg");

            iGlyphMoney = new CustomVertex.PositionTextured[6];
            iGlyphMoney[0].Position = new Vector3(-1.5F, -0.64F, 0);
            iGlyphMoney[1].Position = new Vector3(1.5F, -0.64F, 0);
            iGlyphMoney[2].Position = new Vector3(-1.5F, 0.64F, 0);

            iGlyphMoney[3].Position = new Vector3(-1.5F, 0.64F, 0);
            iGlyphMoney[4].Position = new Vector3(1.5F, -0.64F, 0);
            iGlyphMoney[5].Position = new Vector3(1.5F, 0.64F, 0);

            //Texture coordinates
            iGlyphMoney[0].Tu = 0;
            iGlyphMoney[0].Tv = 1;

            iGlyphMoney[1].Tu = 1;
            iGlyphMoney[1].Tv = 1;

            iGlyphMoney[2].Tu = 0;
            iGlyphMoney[2].Tv = 0;

            iGlyphMoney[3].Tu = 0;
            iGlyphMoney[3].Tv = 0;

            iGlyphMoney[4].Tu = 1;
            iGlyphMoney[4].Tv = 1;

            iGlyphMoney[5].Tu = 1;
            iGlyphMoney[5].Tv = 0;
        }

        private void DrawGlyph(float aValue, string aType)
        {
            _device.RenderState.Lighting = false;
            _device.VertexFormat = CustomVertex.PositionTextured.Format;

            //Setup blending
            _device.TextureState[0].AlphaOperation = TextureOperation.Modulate;
            _device.TextureState[0].AlphaArgument1 = TextureArgument.TextureColor;
            _device.TextureState[0].AlphaArgument2 = TextureArgument.TFactor;
            _device.RenderState.AlphaBlendEnable = true;

            //Save states
            Material oldMaterial = _device.Material;
            Matrix oldTransform = _device.Transform.World;

            //Setup backdrop
            Material _material = new Material();
            Color backcolor = Color.FromArgb(0, 0, 100); //colorMap.GetColor(i * 25);

            //Money
            //-----
            int maxValue = 7;
            int lowColor = 20; //0-255
            int colDiff = ((255 - lowColor) / maxValue);
            _device.SetTexture(0, iTexture);

            for (int i = 0; i < maxValue; i++)
            {
                //Translate
                _device.Transform.World *= Matrix.Translation(0.1F, 0.2F, 0);

                //1. Draw Background
                _device.RenderState.Lighting = true;
                _device.TextureState[0].AlphaOperation = TextureOperation.Disable;

                _material.Emissive = backcolor;
                _material.Diffuse = backcolor;
                _device.Material = _material;
                _device.DrawUserPrimitives(PrimitiveType.TriangleList, 2, iGlyphMoney);

                //2. Draw texture with blending
                _device.RenderState.Lighting = false;
                _device.TextureState[0].AlphaOperation = TextureOperation.Modulate;
                _device.RenderState.TextureFactor = Color.FromArgb(colDiff * i + lowColor, 255, 255, 255).ToArgb();
                _device.DrawUserPrimitives(PrimitiveType.TriangleList, 2, iGlyphMoney);
            }

            //Restore Previous values
            _device.Material = oldMaterial;
            _device.Transform.World = oldTransform;

            //restore texture changes
            _device.VertexFormat = CustomVertex.PositionColored.Format;
            _device.SetTexture(0, null);
            _device.RenderState.AlphaBlendEnable = false;
            _device.TextureState[0].AlphaOperation = TextureOperation.Disable;
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

            Color c = Color.Black;


            // Get the mapped values from the first axis map. This axis map is connected to the first column in the data cube.
            // The index corresponds to the column in the data cube.
            float[] mappedValues = _axisMaps[0].MappedValues;

            // Tells the device that the next primitives to draw are of type CustomVertex.PositionOnly.
            _device.VertexFormat = CustomVertex.PositionOnly.Format;

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
                    DrawGlyph(0.4F, "empty");
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

            CreateAxisMaps();

            colorMap = CreateColorMap();
            colorMap.Input = _input.GetData();
            colorMap.Index = 4;

            InitMapGlyphs();

            _inited = true;
        }

        private ColorMap CreateColorMap()
        {
            ColorMap map = new ColorMap();
            LinearColorMapPart linearMap = new LinearColorMapPart(Color.Black, Color.White);
            map.AddColorMapPart(linearMap);
            linearMap.Invalidate();
            map.Invalidate();

            return map;
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

        protected override void InternalInvalidate() { }
    }
}