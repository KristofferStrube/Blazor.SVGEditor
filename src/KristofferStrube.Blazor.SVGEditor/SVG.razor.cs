﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using BlazorContextMenu;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace KristofferStrube.Blazor.SVGEditor
{
    public partial class SVG : ComponentBase
    {
        [Parameter]
        public string Input { get; set; }

        private string _Input { get; set; }

        [Parameter]
        public Action<string> InputUpdated { get; set; }

        private IDocument Document { get; set; }

        public Shape ColorPickerShape { get; set; }

        public string ColorPickerAttribute { get; set; }

        public string ColorPickerTitle => $"Pick {ColorPickerAttribute} Color";

        public bool IsColorPickerOpen => ColorPickerShape is not null;

        public string PreviousColor => ColorPickerShape is not null ? (ColorPickerAttribute == "Fill" ? ColorPickerShape.Fill : ColorPickerShape.Stroke) : String.Empty;

        protected override async Task OnParametersSetAsync()
        {
            if (Input == _Input)
            {
                return;
            }
            _Input = Input;

            var config = Configuration.Default;

            var context = BrowsingContext.New(config);

            Document = await context.OpenAsync(req => req.Content(Input));

            Elements = Document.GetElementsByTagName("BODY")[0].Children.Select<AngleSharp.Dom.IElement, ISVGElement>(child =>
            {
                ISVGElement element;
                switch (child.TagName)
                {
                    case "RECT":
                        element = new Rectangle(child, this);
                        break;
                    case "CIRCLE":
                        element = new Circle(child, this);
                        break;
                    case "POLYGON":
                        element = new Polygon(child, this);
                        break;
                    case "PATH":
                        element = new Path(child, this);
                        break;
                    default:
                        element = new NonImplmentedElement();
                        break;

                }
                element.Changed = UpdateInput;
                return element;
            }
            ).ToList();

            ElementsAsHtml = Elements.Select(e => e.Element.ToHtml()).ToList();
        }

        protected override void OnInitialized()
        {
            ElementSubject
                .Buffer(TimeSpan.FromMilliseconds(33))
                .Where(updates => updates.Count > 0)
                .Subscribe(updates =>
                {
                    updates
                        .DistinctBy(element => Elements.IndexOf(element))
                        .ToList()
                        .ForEach(element =>
                        {
                            ElementsAsHtml[Elements.IndexOf(element)] = element.Element.ToHtml();
                        });
                    _Input = string.Join(" \n", ElementsAsHtml);
                    InputUpdated(_Input);
                });
        }

        private Subject<ISVGElement> ElementSubject = new();

        private void UpdateInput(ISVGElement element)
        {
            ElementSubject.OnNext(element);
        }

        public double Scale { get; set; } = 1;

        public List<ISVGElement> Elements { get; set; }

        public List<string> ElementsAsHtml { get; set; }

        public ISVGElement CurrentShape { get; set; }

        public void Move(MouseEventArgs eventArgs)
        {
            CurrentShape?.HandleMouseMove(eventArgs);
        }

        public void Up(MouseEventArgs eventArgs)
        {
            CurrentShape?.HandleMouseUp(eventArgs);
        }

        public void UnSelect(MouseEventArgs eventArgs)
        {
            if (CurrentShape != null && CurrentShape.EditMode != EditMode.Add)
            CurrentShape = null;
        }

        public void Out(MouseEventArgs eventArgs)
        {
            CurrentShape?.HandleMouseOut(eventArgs);
        }

        public void ZoomIn(ItemClickEventArgs e)
        {
            if (Scale >= 0.5)
            {
                Scale += 0.1 + 0.1 * (int)(Scale / 2);
            }
            else
            {
                Scale *= 1.1;
                if (Scale > 0.47 && Scale < 0.53)
                {
                    Scale = 0.5;
                }
            }
        }

        public void ZoomOut(ItemClickEventArgs e)
        {
            if (Scale > 0.5)
            {
                Scale -= 0.1 + 0.1 * (int)(Scale / 2);
            }
            else
            {
                Scale /= 1.1;
            }
        }

        protected void OpenFillColorPicker(ItemClickEventArgs e)
        {
            ColorPickerShape = (Shape)e.Data;
            ColorPickerAttribute = "Fill";
        }

        protected void OpenStrokeColorPicker(ItemClickEventArgs e)
        {
            ColorPickerShape = (Shape)e.Data;
            ColorPickerAttribute = "Stroke";
        }

        protected void ColorPickerClosed(string value)
        {
            if (ColorPickerAttribute == "Fill")
            {
                ColorPickerShape.Fill = value;
            }
            else if (ColorPickerAttribute == "Stroke")
            {
                ColorPickerShape.Stroke = value;
            }
            ColorPickerShape = null;
        }

        protected void AddNewPath(ItemClickEventArgs e)
        {
            var element = Document.CreateElement("PATH");
            var path = new Path(element, this);
            path.Changed = UpdateInput;

            path.Stroke = "black";
            path.StrokeWidth = "1";
            path.Fill = "lightgrey";

            Elements.Add(path);
            CurrentShape = path;
            path.EditMode = EditMode.Add;
            ElementsAsHtml.Add(element.ToHtml());
            _Input = string.Join(" \n", ElementsAsHtml);
            InputUpdated(_Input);
        }

        protected void CompleteShape(ItemClickEventArgs e)
        {
            CurrentShape.EditMode = EditMode.None;
            switch (CurrentShape)
            {
                case Path path:
                    path.Instructions.RemoveAt(path.Instructions.Count - 1);
                    path.Instructions.RemoveAt(path.Instructions.Count - 1);
                    path.Instructions.Add(new ClosePathInstruction(false, path.Instructions.Last()));
                    ElementsAsHtml[Elements.IndexOf(path)] = path.Element.ToHtml();
                    _Input = string.Join(" \n", ElementsAsHtml);
                    InputUpdated(_Input);
                    break;
            }
            CurrentShape = null;
        }
    }
}