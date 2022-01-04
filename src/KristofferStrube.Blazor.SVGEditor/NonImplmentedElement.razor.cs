using AngleSharp.Dom;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Runtime.CompilerServices;

namespace KristofferStrube.Blazor.SVGEditor
{
    partial class NonImplmentedElement : ComponentBase, ISVGElement
    {
        public IElement Element { get; set; }

        public bool Selected => throw new NotImplementedException();


        public string TagName { get; set; }

        public void HandleMouseMove(MouseEventArgs eventArgs)
        {
            throw new NotImplementedException();
        }

        public void HandleMouseOut(MouseEventArgs eventArgs)
        {
            throw new NotImplementedException();
        }

        public void HandleMouseUp(MouseEventArgs eventArgs)
        {
            throw new NotImplementedException();
        }

        public Action<ISVGElement> Changed { get; set; }

        public Type Editor => throw new NotImplementedException();

        IElement ISVGElement.Element { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        Type ISVGElement.Editor => throw new NotImplementedException();

        bool ISVGElement.Selected => throw new NotImplementedException();

        string ISVGElement._StateRepresentation { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        Action<ISVGElement> ISVGElement.Changed { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public BoundingBox BoundingBox { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string StoredHtml { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            Changed.Invoke(this);
        }

        public static Action<SVG> AddNew = SVG =>
        {
        };

        public void Complete()
        {
        }

        void ISVGElement.HandleMouseMove(MouseEventArgs eventArgs)
        {
            throw new NotImplementedException();
        }

        void ISVGElement.HandleMouseUp(MouseEventArgs eventArgs)
        {
            throw new NotImplementedException();
        }

        void ISVGElement.HandleMouseOut(MouseEventArgs eventArgs)
        {
            throw new NotImplementedException();
        }

        void ISVGElement.Complete()
        {
            throw new NotImplementedException();
        }

        public string UpdateHtml()
        {
            throw new NotImplementedException();
        }

        public void Rerender()
        {
            throw new NotImplementedException();
        }

        void ISVGElement.UpdateHtml()
        {
            throw new NotImplementedException();
        }
    }
}
