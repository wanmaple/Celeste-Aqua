using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Celeste.Mod.Aqua.Rendering
{
    public class Cube
    {
        public VertexPositionColorTexture[] Vertices => _vertexes;

        public Cube()
            : this(1.0f, 1.0f, 1.0f)
        {
        }

        public Cube(float xLen, float yLen, float zLen)
        {
            float xHalf = xLen * 0.5f, yHalf = yLen * 0.5f, zHalf = zLen * 0.5f;
            Vector3[] positions = new Vector3[]
            {
                new Vector3(-xHalf, -yHalf, zHalf),
                new Vector3(xHalf, -yHalf, zHalf),
                new Vector3(xHalf, yHalf, zHalf),
                new Vector3(-xHalf, yHalf, zHalf),
                new Vector3(-xHalf, -yHalf, -zHalf),
                new Vector3(xHalf, -yHalf, -zHalf),
                new Vector3(xHalf, yHalf, -zHalf),
                new Vector3(-xHalf, yHalf, -zHalf),
            };
            int[] indices = new int[]
            {
                1, 3, 0, 1, 2, 3,
                4, 6, 5, 4, 7, 6,
                0, 7, 4, 0, 3, 7,
                5, 2, 1, 5, 6, 2,
                0, 5, 1, 0, 4, 5,
                7, 2, 6, 7, 3, 2,
            };
            Vector2[] uvs = new Vector2[]
            {
                Vector2.Zero,
                Vector2.One,
                Vector2.UnitX,
                Vector2.Zero,
                Vector2.UnitY,
                Vector2.One,
            };
            _vertexes = new VertexPositionColorTexture[36];
            for (int i = 0; i < _vertexes.Length; i++)
            {
                VertexPositionColorTexture vert = new VertexPositionColorTexture
                {
                    Position = positions[indices[i]],
                    Color = Color.White,
                    TextureCoordinate = uvs[i % 6],
                };
                _vertexes[i] = vert;
            }
        }

        private VertexPositionColorTexture[] _vertexes;
    }
}
