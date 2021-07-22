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
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            Changed.Invoke(this);
        }
    }
}
