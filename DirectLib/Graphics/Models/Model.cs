﻿using DirectLib.Graphics.Data;
using DirectLib.System;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Globalization;

namespace DirectLib.Graphics.Models
{
    public class Model                 // 202 lines
    {
        // Structures
        [StructLayout(LayoutKind.Sequential)]
        public struct ModelFormat
        {
            public float x, y, z;
            public float tu, tv;
            public float nx, ny, nz;
        }
        [StructLayout(LayoutKind.Sequential)]
        internal struct VertexModel
        {
            public Vector3 position;
            public Vector2 texture;
            public Vector3 normal;
        }

        // Properties
        private SharpDX.Direct3D11.Buffer VertexBuffer { get; set; }
        private SharpDX.Direct3D11.Buffer IndexBuffer { get; set; }
        private int VertexCount { get; set; }
        public int IndexCount { get; private set; }
        public Texture Texture { get; set; }
        public ModelFormat[] ModelObject { get; protected set; }

        // Constructor 
        public Model() { }

        // Methods
        public virtual bool Initialize(SharpDX.Direct3D11.Device device, string modelFormatFilename, string textureFileNames, Vector3 scale)
        {
            // Load in the model data.
            if (!LoadModel(modelFormatFilename, scale))
                return false;

            // Initialize the vertex and index buffer.
            if (!InitializeBuffers(device))
                return false;

            // Load the texture for this model.  no Textures in this Tutporial 35
            if (!LoadTextures(device, textureFileNames))
                return false;

            return true;
        }
        protected bool LoadModel(string modelFormatFilename, Vector3 scale)
        {
            string filename = modelFormatFilename;
            modelFormatFilename = SystemConfiguration.ModelFilePath + modelFormatFilename;
            List<string> lines = null;

            try
            {
                lines = File.ReadLines(modelFormatFilename).ToList();

                var vertexCountString = lines[0].Split(new char[] { ':' })[1].Trim();
                VertexCount = int.Parse(vertexCountString);
                IndexCount = VertexCount;
                ModelObject = new ModelFormat[VertexCount];

                for (var i = 4; i < lines.Count && i < 4 + VertexCount; i++)
                {
                    var modelArray = lines[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                    ModelObject[i - 4] = new ModelFormat()
                    {
                        x = scale.X * float.Parse(modelArray[0], CultureInfo.GetCultureInfo("En-en")),
                        y = scale.Y * float.Parse(modelArray[1], CultureInfo.GetCultureInfo("En-en")),
                        z = scale.Z * float.Parse(modelArray[2], CultureInfo.GetCultureInfo("En-en")),
                        tu = float.Parse(modelArray[3], CultureInfo.GetCultureInfo("En-en")),
                        tv = float.Parse(modelArray[4], CultureInfo.GetCultureInfo("En-en")),
                        nx = float.Parse(modelArray[5], CultureInfo.GetCultureInfo("En-en")),
                        ny = float.Parse(modelArray[6], CultureInfo.GetCultureInfo("En-en")),
                        nz = float.Parse(modelArray[7], CultureInfo.GetCultureInfo("En-en"))
                    };
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        protected bool LoadTextures(SharpDX.Direct3D11.Device device, string textureFileNames)
        {
            textureFileNames = SystemConfiguration.DataFilePath + textureFileNames;

            // Create the texture object.
            Texture = new Texture();

            // Initialize the texture object.
            Texture.Initialize(device, textureFileNames);

            return true;
        }
        public void Shutdown()
        {
            // Release the model texture.
            ReleaseTextures();

            // Release the vertex and index buffers.
            ShutdownBuffers();

            // Release the model data.
            ReleaseModel();
        }
        protected void ReleaseModel()
        {
            ModelObject = null;
        }
        // Modified in Tutorial 18 for Light Maps.
        private void ReleaseTextures()
        {
            // Release the textures object.
            Texture?.ShutDown();
            Texture = null;
        }
        public void Render(SharpDX.Direct3D11.DeviceContext deviceContext)
        {
            // Put the vertex and index buffers on the graphics pipeline to prepare for drawings.
            RenderBuffers(deviceContext);
        }
        protected bool InitializeBuffers(SharpDX.Direct3D11.Device device)
        {
            try
            {
                // Create the vertex array.
                var vertices = new VertexModel[VertexCount];
                // Create the index array.
                var indices = new int[IndexCount];

                for (var i = 0; i < VertexCount; i++)
                {
                    vertices[i] = new VertexModel()
                    {
                        position = new Vector3(ModelObject[i].x, ModelObject[i].y, ModelObject[i].z),
                        texture = new Vector2(ModelObject[i].tu, ModelObject[i].tv),
                        normal = new Vector3(ModelObject[i].nx, ModelObject[i].ny, ModelObject[i].nz)
                    };

                    indices[i] = i;
                }

                // Create the vertex buffer.
                VertexBuffer = SharpDX.Direct3D11.Buffer.Create(device, BindFlags.VertexBuffer, vertices);

                // Create the index buffer.
                IndexBuffer = SharpDX.Direct3D11.Buffer.Create(device, BindFlags.IndexBuffer, indices);

                return true;
            }
            catch
            {
                return false;
            }
        }
        protected void ShutdownBuffers()
        {
            // Return the index buffer.
            IndexBuffer?.Dispose();
            IndexBuffer = null;
            // Release the vertex buffer.
            VertexBuffer?.Dispose();
            VertexBuffer = null;
        }
        protected void RenderBuffers(SharpDX.Direct3D11.DeviceContext deviceContext)
        {
            // Set the vertex buffer to active in the input assembler so it can be rendered.
            deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VertexBuffer, Utilities.SizeOf<VertexModel>(), 0));
            // Set the index buffer to active in the input assembler so it can be rendered.
            deviceContext.InputAssembler.SetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);
            // Set the type of the primitive that should be rendered from this vertex buffer, in this case triangles.
            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
        }
    }
}