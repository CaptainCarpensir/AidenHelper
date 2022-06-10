using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace AidenHelper.Entities
{
	public class AidenHelperSinkingPlatformLine : Entity
	{
		private Color lineEdgeColor;
		private Color lineInnerColor;
		private Vector2 midPos;

		private float height;

		public AidenHelperSinkingPlatformLine(Vector2 pos1, Vector2 pos2, string innerColor, string outerColor)
		{
			Position = pos1;
			midPos = pos2;
			base.Depth = 9001;
			lineEdgeColor = Calc.HexToColor(outerColor);
			lineInnerColor = Calc.HexToColor(innerColor);
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			//height = (float)SceneAs<Level>().Bounds.Height - (base.Y - (float)SceneAs<Level>().Bounds.Y);
			height = -(Position.Y - midPos.Y);
		}

		public override void Render()
		{
			Draw.Rect(base.X - 1f, base.Y, 3f, height, lineEdgeColor);
			Draw.Rect(base.X, base.Y + 1f, 1f, height, lineInnerColor);

			Draw.Rect(base.X - 2f, midPos.Y - 2f, 5f, 5f, lineEdgeColor);
			Draw.Rect(base.X - 1f, midPos.Y - 1f, 3f, 3f, lineInnerColor);

			Draw.Rect(base.X - 2f, base.Y - 2f, 5f, 5f, lineEdgeColor);
			Draw.Rect(base.X - 1f, base.Y - 1f, 3f, 3f, lineInnerColor);
		}
	}
}
