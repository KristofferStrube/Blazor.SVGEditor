namespace KristofferStrube.Blazor.SVGEditor
{
    public abstract class BaseControlPointPathInstruction : BasePathInstruction
    {
        public BaseControlPointPathInstruction(bool Relative, IPathInstruction PreviousInstruction) : base(Relative, PreviousInstruction)
        {
            UpdateReflectedPreviousInstructionsLastControlPoint();
        }
        public List<(double x, double y)> ControlPoints { get; set; } = new();
        public (double x, double y) ReflectedPreviousInstructionsLastControlPoint
        {
            get
            {
                return _reflectedPreviousInstructionsLastControlPoint;
            }
            set
            {
                _reflectedPreviousInstructionsLastControlPoint = value;
                if (PreviousInstruction is BaseControlPointPathInstruction controlPointInstruction)
                {
                    if (controlPointInstruction.ControlPoints.Count != 0)
                    {
                        controlPointInstruction.ControlPoints[^1] = (StartPosition.x * 2 - value.x, StartPosition.y * 2 - value.y);
                    }
                    else
                    {
                        controlPointInstruction.ReflectedPreviousInstructionsLastControlPoint = (StartPosition.x * 2 - value.x, StartPosition.y * 2 - value.y);
                    }
                }
                UpdateReflectionForInstructions();
            }
        }

        public void UpdateReflectionForInstructions()
        {
            UpdateReflectedPreviousInstructionsLastControlPoint();
            if (NextInstruction is not null and BaseControlPointPathInstruction reflectedControlPointInstruction)
            {
                reflectedControlPointInstruction.UpdateReflectionForInstructions();
            }
        }

        private void UpdateReflectedPreviousInstructionsLastControlPoint()
        {
            if (PreviousInstruction is BaseControlPointPathInstruction controlPointInstruction)
            {
                if (controlPointInstruction.ControlPoints.Count != 0)
                {
                    _reflectedPreviousInstructionsLastControlPoint = (StartPosition.x * 2 - controlPointInstruction.ControlPoints[^1].x, StartPosition.y * 2 - controlPointInstruction.ControlPoints[^1].y);
                }
                else
                {
                    _reflectedPreviousInstructionsLastControlPoint = (StartPosition.x * 2 - controlPointInstruction.ReflectedPreviousInstructionsLastControlPoint.x, StartPosition.y * 2 - controlPointInstruction.ReflectedPreviousInstructionsLastControlPoint.y);
                }
            }
            else
            {
                _reflectedPreviousInstructionsLastControlPoint = StartPosition;
            }
        }

        private (double x, double y) _reflectedPreviousInstructionsLastControlPoint { get; set; }
    }
}
