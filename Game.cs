using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;

namespace WindowEngine
{
    public class Game
    {
        private readonly Surface screen;
        private int vao, vbo, shaderProgram;
        private int modelLoc;
        private float rotationAngle = 0f;

        public Game(int width, int height)
        {
            screen = new Surface(width, height);
        }

        public void Init()
        {
            // Set clear color to white
            GL.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);

            // Simple vertex data for a square (-1 to 1 coordinates)
            float[] vertices = {
                // Positions         // Colors
                -0.5f, -0.5f, 0.0f,  1.0f, 0.0f, 0.0f,  // Bottom-left: red
                 0.5f, -0.5f, 0.0f,  0.0f, 1.0f, 0.0f,  // Bottom-right: green
                 0.5f,  0.5f, 0.0f,  0.0f, 0.0f, 1.0f,  // Top-right: blue
                -0.5f,  0.5f, 0.0f,  1.0f, 1.0f, 0.0f   // Top-left: yellow
            };

            // Create and bind VAO and VBO
            vao = GL.GenVertexArray();
            vbo = GL.GenBuffer();

            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            // Vertex attributes
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            // Simple vertex shader
            string vertexShaderSource = @"
                #version 330 core
                layout(location = 0) in vec3 aPosition;
                layout(location = 1) in vec3 aColor;
                out vec3 vColor;
                uniform mat4 uModel;
                void main()
                {
                    gl_Position = uModel * vec4(aPosition, 1.0);
                    vColor = aColor;
                }";

            // Fragment shader
            string fragmentShaderSource = @"
                #version 330 core
                in vec3 vColor;
                out vec4 FragColor;
                void main()
                {
                    FragColor = vec4(vColor, 1.0);
                }";

            // Compile shaders
            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexShaderSource);
            GL.CompileShader(vertexShader);
            CheckShaderError(vertexShader, "Vertex Shader");

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentShaderSource);
            GL.CompileShader(fragmentShader);
            CheckShaderError(fragmentShader, "Fragment Shader");

            // Link shader program
            shaderProgram = GL.CreateProgram();
            GL.AttachShader(shaderProgram, vertexShader);
            GL.AttachShader(shaderProgram, fragmentShader);
            GL.LinkProgram(shaderProgram);
            CheckProgramError(shaderProgram);

            // Clean up shaders
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            // Get uniform location
            GL.UseProgram(shaderProgram);
            modelLoc = GL.GetUniformLocation(shaderProgram, "uModel");

            CheckGLError("After Init");
        }

        public void Tick()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // Update rotation angle
            rotationAngle += 0.01f;

            RenderGL();
            CheckGLError("After Tick");
        }

        private void RenderGL()
        {
            // Create rotation matrix
            Matrix4 rotationMatrix = Matrix4.CreateRotationZ(rotationAngle);

            // Use shader program
            GL.UseProgram(shaderProgram);

            // Send matrix to shader
            GL.UniformMatrix4(modelLoc, false, ref rotationMatrix);

            // Draw the square
            GL.BindVertexArray(vao);
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);
        }

        public void Cleanup()
        {
            GL.DeleteBuffer(vbo);
            GL.DeleteVertexArray(vao);
            GL.DeleteProgram(shaderProgram);
        }

        private void CheckGLError(string context)
        {
            ErrorCode error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                Console.WriteLine($"OpenGL Error at {context}: {error}");
            }
        }

        private void CheckShaderError(int shader, string name)
        {
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(shader);
                Console.WriteLine($"{name} Compilation Error: {infoLog}");
            }
        }

        private void CheckProgramError(int program)
        {
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetProgramInfoLog(program);
                Console.WriteLine($"Program Link Error: {infoLog}");
            }
        }
    }

    public class Surface
    {
        public int[] pixels;
        public int width, height;

        public Surface(int width, int height)
        {
            this.width = width;
            this.height = height;
            pixels = new int[width * height];
        }
    }
}