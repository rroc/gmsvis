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
        private List<Texture> iTexture;
        private Material iMaterial;
        private CustomVertex.PositionTextured[] iBasicGlyph;
        private CustomVertex.PositionTextured[] iAgeGlyph;

        private bool _inited;

        private IDataCubeProvider<float> _input;

        private System.Drawing.Font iSystemFont;
        private Microsoft.DirectX.Direct3D.Font iD3dFont;


        private List<AxisMap> _axisMaps;

        public IDataCubeProvider<float> Input
        {
            get { return _input; }
            set { _input = value; }
        }



        //----------------------------------------------------------

        private void InitMapGlyphs()
        {
            iSystemFont = new System.Drawing.Font("Verdana", 10);
            iD3dFont = new Microsoft.DirectX.Direct3D.Font(_device, iSystemFont);

            iMaterial = new Material();

            iMaterial.Diffuse = Color.White;
            iMaterial.Specular = Color.LightGray;
            iMaterial.SpecularSharpness = 15.0F;

            _device.Material = iMaterial;

            string dir = Directory.GetCurrentDirectory();
            string dataPath = "\\..\\..\\..\\data\\glyphs\\";

            iTexture = new List<Texture>();
            iTexture.Add( TextureLoader.FromFile(_device, dir+dataPath+"coin.png") );
            iTexture.Add( TextureLoader.FromFile(_device, dir + dataPath + "cd.png") );
            iTexture.Add(TextureLoader.FromFile(_device, dir + dataPath + "age.png"));
            iTexture.Add(TextureLoader.FromFile(_device, dir + dataPath + "work.png"));

            float scale = 10;
            iBasicGlyph = new CustomVertex.PositionTextured[6];
            iBasicGlyph[0].Position = new Vector3(scale * -1.5F, scale * -0.64F, 0);
            iBasicGlyph[1].Position = new Vector3(scale * 1.5F, scale * -0.64F, 0);
            iBasicGlyph[2].Position = new Vector3(scale * -1.5F, scale * 0.64F, 0);

            iBasicGlyph[3].Position = new Vector3(scale * -1.5F, scale * 0.64F, 0);
            iBasicGlyph[4].Position = new Vector3(scale * 1.5F, scale * -0.64F, 0);
            iBasicGlyph[5].Position = new Vector3(scale * 1.5F, scale * 0.64F, 0);

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


            iAgeGlyph = new CustomVertex.PositionTextured[6];
            iAgeGlyph[0].Position = new Vector3(scale * -1.5F, scale * -1.0F, 0);
            iAgeGlyph[1].Position = new Vector3(scale * 1.5F, scale * -1.0F, 0);
            iAgeGlyph[2].Position = new Vector3(scale * -1.5F, scale * 1.0F, 0);

            iAgeGlyph[3].Position = new Vector3(scale * -1.5F, scale * 1.0F, 0);
            iAgeGlyph[4].Position = new Vector3(scale * 1.5F, scale * -1.0F, 0);
            iAgeGlyph[5].Position = new Vector3(scale * 1.5F, scale * 1.0F, 0);

            //Texture coordinates
            iAgeGlyph[0].Tu = 0;
            iAgeGlyph[0].Tv = 1;

            iAgeGlyph[1].Tu = 1;
            iAgeGlyph[1].Tv = 1;

            iAgeGlyph[2].Tu = 0;
            iAgeGlyph[2].Tv = 0;

            iAgeGlyph[3].Tu = 0;
            iAgeGlyph[3].Tv = 0;

            iAgeGlyph[4].Tu = 1;
            iAgeGlyph[4].Tv = 1;

            iAgeGlyph[5].Tu = 1;
            iAgeGlyph[5].Tv = 0;
        }

        private void DrawGlyph(int aItem )
        {
            _device.RenderState.Lighting = false;
            _device.VertexFormat = CustomVertex.PositionTextured.Format;

            //Setup blending
            _device.TextureState[0].AlphaOperation = TextureOperation.Modulate;
            _device.TextureState[0].AlphaArgument1 = TextureArgument.TextureColor;
            _device.TextureState[0].AlphaArgument2 = TextureArgument.TFactor;
            _device.RenderState.AlphaBlendEnable = true;

            //Save states
            Matrix oldTransform = _device.Transform.World;
            _device.RenderState.Lighting = false;
            int maxValue;
            float count;
            int maxAmount;

            //MUSIC RELEASES
            //-----
            //Translate
            _device.Transform.World = oldTransform;
            _device.Transform.World *= Matrix.Translation(9.0F, 2.0F, 0);

            maxValue = 9;
            _device.SetTexture(0, iTexture[2]);

            count = _axisMaps[2].MappedValues[aItem];
            maxAmount = 1 + (int)(maxValue * count);

            for (int i = 0; i < maxAmount; i++)
            {
                //Translate
                _device.Transform.World *= Matrix.Translation(0.0F, 3.0F, 0);
                _device.DrawUserPrimitives(PrimitiveType.TriangleList, 2, iAgeGlyph);
            }

            //Restore Previous values
            _device.Transform.World = oldTransform;


            //MONEY (GDP)
            //-----
            //Translate
            _device.Transform.World = oldTransform;
            _device.Transform.World *= Matrix.Translation(-9.0F, -2.0F, 0);

            maxValue = 9;
            _device.SetTexture(0, iTexture[3]);

            count = _axisMaps[4].MappedValues[aItem];
            maxAmount = 1+(int)(maxValue * count);

            for (int i = 0; i < maxAmount; i++)
            {
                //Translate
                _device.Transform.World *= Matrix.Translation(0.0F, 3.0F, 0);
                _device.DrawUserPrimitives(PrimitiveType.TriangleList, 2, iBasicGlyph);
            }


            //Restore Previous values
            _device.Transform.World = oldTransform;

            //restore texture changes
            _device.VertexFormat = CustomVertex.PositionColored.Format;
            _device.SetTexture(0, null);
            _device.RenderState.AlphaBlendEnable = false;
            _device.TextureState[0].AlphaOperation = TextureOperation.Disable;
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
                    DrawGlyph(i);
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