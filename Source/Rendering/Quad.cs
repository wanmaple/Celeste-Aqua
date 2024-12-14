using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Celeste.Mod.Aqua.Rendering
{
    public class Quad
    {
        public VertexPositionColorTexture[] Vertices => _vertexes;

        public Quad()
            : this(1.0f, 1.0f)
        {
        }

        public Quad(float xLen, float yLen)
        {
            float xHalf = xLen * 0.5f, yHalf = yLen * 0.5f;
            Vector3[] positions = new Vector3[]
            {
                new Vector3(-xHalf, -yHalf, 0.0f),
                new Vector3(xHalf, -yHalf, 0.0f),
                new Vector3(xHalf, yHalf, 0.0f),
                new Vector3(-xHalf, yHalf, 0.0f),
            };
            int[] indices = new int[]
            {
                0, 1, 2, 0, 2, 3,
            };
            Vector2[] uvs = new Vector2[]
            {
                Vector2.Zero,
                Vector2.UnitX,
                Vector2.One,
                Vector2.Zero,
                Vector2.One,
                Vector2.UnitY,
            };
            _vertexes = new VertexPositionColorTexture[6];
            for (int i = 0; i < _vertexes.Length; i++)
            {
                VertexPositionColorTexture vert = new VertexPositionColorTexture
                {
                    Position = positions[indices[i]],
                    Color = Color.White,
                    TextureCoordinate = uvs[i],
                };
                _vertexes[i] = vert;
            }
        }

        private VertexPositionColorTexture[] _vertexes;
    }
}
