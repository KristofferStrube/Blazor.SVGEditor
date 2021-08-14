using AngleSharp.Dom;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace KristofferStrube.Blazor.SVGEditor
{
    partial class NonImplmentedElement : ComponentBase, ISVGElement
    {
        public IElement Element { get; set; }

        public bool Selected => throw new NotImplementedException();

        public bool Selectable => throw new NotImplementedException();

        public EditMode EditMode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string TagName { get; set; }

        public IEnumerable<EditMode> AvailableEditModes { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public (double x, double y) Panner { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

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

        bool ISVGElement.Selectable => throw new NotImplementedException();

        EditMode ISVGElement.EditMode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        IEnumerable<EditMode> ISVGElement.AvailableEditModes { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        (double x, double y) ISVGElement.Panner { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        string ISVGElement._StateRepresentation { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        Action<ISVGElement> ISVGElement.Changed { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

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
    }
}
