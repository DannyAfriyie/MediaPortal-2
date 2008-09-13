#region Copyright (C) 2007-2008 Team MediaPortal

/*
    Copyright (C) 2007-2008 Team MediaPortal
    http://www.team-mediaportal.com
 
    This file is part of MediaPortal II

    MediaPortal II is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal II is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal II.  If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using System;
using System.Drawing;
using MediaPortal.Control.InputManager;
using MediaPortal.Presentation.DataObjects;
using MediaPortal.SkinEngine.InputManagement;
using SlimDX;
using SlimDX.Direct3D9;
using MediaPortal.SkinEngine.Controls.Visuals;
using MediaPortal.Utilities.DeepCopy;
using MediaPortal.SkinEngine.SkinManagement;

namespace MediaPortal.SkinEngine.Controls.Panels
{
  public class StackPanel : Panel, IScrollInfo
  {
    #region Private fields

    Property _orientationProperty;
    bool _isScrolling;
    float _totalHeight;
    float _totalWidth;
    float _physicalScrollOffsetY = 0;
    float _physicalScrollOffsetX = 0;

    #endregion

    #region Ctor

    public StackPanel()
    {
      Init();
      Attach();
    }

    void Init()
    {
      _orientationProperty = new Property(typeof(Orientation), Orientation.Vertical);
    }

    void Attach()
    {
      _orientationProperty.Attach(OnPropertyInvalidate);
    }

    void Detach()
    {
      _orientationProperty.Detach(OnPropertyInvalidate);
    }

    public override void DeepCopy(IDeepCopyable source, ICopyManager copyManager)
    {
      Detach();
      base.DeepCopy(source, copyManager);
      StackPanel p = source as StackPanel;
      Orientation = copyManager.GetCopy(p.Orientation);
      Attach();
    }

    #endregion

    #region Public properties

    public Property OrientationProperty
    {
      get { return _orientationProperty; }
    }

    public Orientation Orientation
    {
      get { return (Orientation)_orientationProperty.GetValue(); }
      set { _orientationProperty.SetValue(value); }
    }

    #endregion

    #region Measure and arrange

    public override void Measure(ref SizeF totalSize)
    {

      if (LayoutTransform != null)
      {
        ExtendedMatrix m;
        LayoutTransform.GetTransform(out m);
        SkinContext.AddLayoutTransform(m);
      }

      _totalHeight = 0.0f;
      _totalWidth = 0.0f;
      SizeF childSize = new SizeF(0, 0);
      foreach (UIElement child in Children)
      {
        if (!child.IsVisible) 
          continue;
        if (Orientation == Orientation.Vertical)
        {
          child.Measure(ref childSize);
          _totalHeight += childSize.Height;
          if (childSize.Width > _totalWidth)
            _totalWidth = childSize.Width;
        }
        else
        {
          child.Measure(ref childSize);
          _totalWidth += childSize.Width;
          if (childSize.Height > _totalHeight)
            _totalHeight = childSize.Height;
        }
      }

      _desiredSize = new SizeF((float)Width * SkinContext.Zoom.Width, (float)Height * SkinContext.Zoom.Height);

      if (Double.IsNaN(Width))
        _desiredSize.Width = _totalWidth;

      if (Double.IsNaN(Height))
        _desiredSize.Height = _totalHeight;

      if (LayoutTransform != null)
      {
        SkinContext.RemoveLayoutTransform();
      }
      SkinContext.FinalLayoutTransform.TransformSize(ref _desiredSize);

      totalSize = _desiredSize;
      AddMargin(ref totalSize);

      //Trace.WriteLine(String.Format("StackPanel.measure :{0} returns {1}x{2}", this.Name, (int)totalSize.Width, (int)totalSize.Height));
    }

    public override void Arrange(RectangleF finalRect)
    {
      //Trace.WriteLine(String.Format("StackPanel.Arrange :{0} X {1},Y {2} W {3}xH {4}", this.Name, (int)finalRect.X, (int)finalRect.Y, (int)finalRect.Width, (int)finalRect.Height));
      ComputeInnerRectangle(ref finalRect);

      ActualPosition = new Vector3(finalRect.Location.X, finalRect.Location.Y, SkinContext.GetZorder());
      ActualWidth = finalRect.Width;
      ActualHeight = finalRect.Height;

      // Check if content is large than size, then we need to set scrolling to true.
      if (IsItemsHost)
      {
        switch (Orientation)
        {
          case Orientation.Vertical:
            if (_totalHeight > finalRect.Height)
              _isScrolling = true;
            else
            {
              _isScrolling = false;
              _physicalScrollOffsetY = 0;
            }
            break;
          case Orientation.Horizontal:
            if (_totalWidth > finalRect.Width)
              _isScrolling = true;
            else
            {
              _isScrolling = false;
              _physicalScrollOffsetX = 0;
            }
            break;
        }
      }

      if (LayoutTransform != null)
      {
        ExtendedMatrix m;
        LayoutTransform.GetTransform(out m);
        SkinContext.AddLayoutTransform(m);
      }
      switch (Orientation)
      {
        case Orientation.Vertical:
          {
            float totalHeight = 0;
            SizeF Size = new SizeF(0, 0);
            foreach (FrameworkElement child in Children)
            {
              if (!child.IsVisible) 
                continue;

              PointF location = new PointF(ActualPosition.X, ActualPosition.Y + totalHeight);
              
              child.TotalDesiredSize(ref Size);

              // Default behavior is to fill the content if the child has no size
              if (Double.IsNaN(child.Width))
              {
                Size.Width = (float)ActualWidth;
              }

              //align horizontally 
              if (AlignmentX == AlignmentX.Center)
              {
                location.X += ((float)ActualWidth - Size.Width) / 2;
              }
              else if (AlignmentX == AlignmentX.Right)
              {
                location.X = ((float)ActualWidth - Size.Width);
              }

              child.Arrange(new RectangleF(location, Size));
              totalHeight += Size.Height;
            }
          }
          break;

        case Orientation.Horizontal:
          {
            float totalWidth = 0;
            SizeF Size = new SizeF(0, 0);
            foreach (FrameworkElement child in Children)
            {
              if (!child.IsVisible) 
                continue;
              PointF location = new PointF(ActualPosition.X + totalWidth, ActualPosition.Y);
              child.TotalDesiredSize(ref Size);
              
              // Default behavior is to fill the content if the child has no size
              if (Double.IsNaN(child.Height))
              {
                Size.Height = (float)ActualHeight;
              }

              //align vertically 
              if (AlignmentY == AlignmentY.Center)
              {
                location.Y += ((float)ActualHeight - Size.Height) / 2;
              }
              else if (AlignmentY == AlignmentY.Bottom)
              {
                location.Y += ((float)ActualHeight - Size.Height);
              }

              child.Arrange(new RectangleF(location, Size));
              totalWidth += Size.Width;
            }
          }
          break;
      }
      if (LayoutTransform != null)
      {
        SkinContext.RemoveLayoutTransform();
      }
      _finalLayoutTransform = SkinContext.FinalLayoutTransform;

      if (!finalRect.IsEmpty)
      {
        if (_finalRect.Width != finalRect.Width || _finalRect.Height != _finalRect.Height)
          _performLayout = true;
        if (Screen != null) Screen.Invalidate(this);
        _finalRect = new RectangleF(finalRect.Location, finalRect.Size);
      }
      base.Arrange(finalRect);
    }
    #endregion

    #region Rendering

    protected override void RenderChildren()
    {
      lock (_orientationProperty)
      {
        if (_isScrolling)
        {
          GraphicsDevice.Device.ScissorRect = new Rectangle((int)ActualPosition.X, (int)ActualPosition.Y, (int)ActualWidth, (int)ActualHeight);
          GraphicsDevice.Device.SetRenderState(RenderState.ScissorTestEnable, true);
          ExtendedMatrix m = new ExtendedMatrix();
          m.Matrix = Matrix.Translation(new Vector3(0, -_physicalScrollOffsetY, 0));
          SkinContext.AddTransform(m);
        }
        foreach (FrameworkElement element in _renderOrder)
        {
          if (!element.IsVisible) 
            continue;
          float posY = element.ActualPosition.Y - ActualPosition.Y;

          // FIXME Albert78: Fix code for logical scrolling here
          if (_isScrolling)
          {
            // if (posY < _physicalScrollOffsetY) continue;
            // posY -= _physicalScrollOffsetY;
            element.Render();
            // posY += (float)(element.ActualHeight);
            // if (posY > ActualHeight) break;
          }
          else
          {
            element.Render();
          }
        }

        if (_isScrolling)
        {
          GraphicsDevice.Device.SetRenderState(RenderState.ScissorTestEnable, false);
          SkinContext.RemoveTransform();
        }
      }
    }
    #endregion

    #region IScrollInfo Members

    public bool LineDown(PointF point)
    {
      if (Orientation == Orientation.Vertical)
      {
        FrameworkElement focusedElement = FocusManager.FocusedElement;
        if (focusedElement == null) return false;
        FrameworkElement nextElement = PredictFocus(focusedElement.ActualBorders, MoveFocusDirection.Down);
        if (nextElement == null) return false;
        float posY = (float)((nextElement.ActualPosition.Y + nextElement.ActualHeight) - ActualPosition.Y);
        if ((posY - _physicalScrollOffsetY) < ActualHeight) return false;
        _physicalScrollOffsetY += (nextElement.ActualPosition.Y - focusedElement.ActualPosition.Y);
        nextElement.TrySetFocus();
        return true;
      }
      return false;
    }

    public bool LineUp(PointF point)
    {
      if (_physicalScrollOffsetY <= 0) return false;
      if (Orientation == Orientation.Vertical)
      {
        FrameworkElement focusedElement = FocusManager.FocusedElement;
        if (focusedElement == null) return false;
        FrameworkElement prevElement = PredictFocus(focusedElement.ActualBorders, MoveFocusDirection.Up);
        if (prevElement == null) return false;
        if ((prevElement.ActualPosition.Y - ActualPosition.Y) > (_physicalScrollOffsetY)) return false;
        _physicalScrollOffsetY -= (focusedElement.ActualPosition.Y - prevElement.ActualPosition.Y);
        prevElement.TrySetFocus();
        return true;
      }
      return false;
    }

    public bool LineLeft(PointF point)
    {
      return false;
    }

    public bool LineRight(PointF point)
    {
      return false;
    }

    public bool MakeVisible()
    {
      return false;
    }

    public bool PageDown(PointF point)
    {
      FrameworkElement focusedElement = FocusManager.FocusedElement;
      if (focusedElement == null) return false;
      float offsetEnd = (float)(_physicalScrollOffsetY + ActualHeight);
      float y = focusedElement.ActualPosition.Y - (ActualPosition.Y + _physicalScrollOffsetY);
      if (Orientation == Orientation.Vertical)
      {
        while (true)
        {

          if (Orientation == Orientation.Vertical)
          {
            focusedElement = FocusManager.FocusedElement;
            if (focusedElement == null) return false;
            FrameworkElement nextElement = PredictFocus(focusedElement.ActualBorders, MoveFocusDirection.Down);
            if (nextElement == null) return false;
            float posY = (float)((nextElement.ActualPosition.Y + nextElement.ActualHeight) - ActualPosition.Y);
            if ((posY - _physicalScrollOffsetY) < ActualHeight)
            {
              nextElement.TrySetFocus();
            }
            else
            {
              float diff = nextElement.ActualPosition.Y - focusedElement.ActualPosition.Y;
              if (_physicalScrollOffsetY + diff > offsetEnd) break;
              _physicalScrollOffsetY += diff;
              nextElement.TrySetFocus();
            }
          }
        }
        //OnMouseMove((float)point.X, (float)(ActualPosition.Y + y));
      }

      return true;
    }

    public bool PageLeft(PointF point)
    {
      return false;
    }

    public bool PageRight(PointF point)
    {
      return false;
    }

    public bool PageUp(PointF point)
    {
      FrameworkElement focusedElement = FocusManager.FocusedElement;
      if (focusedElement == null) return false;
      float y = focusedElement.ActualPosition.Y - (ActualPosition.Y + _physicalScrollOffsetY);

      float offsetEnd = (float)(_physicalScrollOffsetY - ActualHeight);
      if (offsetEnd <= 0) offsetEnd = 0;
      if (Orientation == Orientation.Vertical)
      {
        while (true)
        {

          if (Orientation == Orientation.Vertical)
          {
            focusedElement = FocusManager.FocusedElement;
            if (focusedElement == null) return false;
            FrameworkElement prevElement = PredictFocus(focusedElement.ActualBorders, MoveFocusDirection.Up);
            if (prevElement == null) return false;
            if ((prevElement.ActualPosition.Y - ActualPosition.Y) > (_physicalScrollOffsetY))
            {
              prevElement.TrySetFocus();
            }
            else
            {
              float diff = focusedElement.ActualPosition.Y - prevElement.ActualPosition.Y;
              if ((_physicalScrollOffsetY - diff) < offsetEnd) break;
              _physicalScrollOffsetY -= diff;
              prevElement.TrySetFocus();
            }
          }
        }
        //OnMouseMove((float)point.X, (float)(ActualPosition.Y + y));
      }

      return true;
    }

    public double LineHeight
    {
      get { return 1000; }
    }

    public double LineWidth
    {
      get { return 0; }
    }

    public bool ScrollToItemWhichStartsWith(string text, int offset)
    {
      return false;
    }

    public void Home(PointF point)
    {
      FrameworkElement focusedElement = FocusManager.FocusedElement;
      if (focusedElement == null) return;
      _physicalScrollOffsetY = 0;
      TrySetFocus();
    }

    public void End(PointF point)
    {
      FrameworkElement focusedElement = FocusManager.FocusedElement;
      if (focusedElement == null) return;
      float offsetEnd = (float)(_totalHeight - ActualHeight);
      float y = focusedElement.ActualPosition.Y - (ActualPosition.Y + _physicalScrollOffsetY);
      if (Orientation == Orientation.Vertical)
      {
        while (true)
        {

          if (Orientation == Orientation.Vertical)
          {
            focusedElement = FocusManager.FocusedElement;
            if (focusedElement == null) return;
            FrameworkElement nextElement = PredictFocus(focusedElement.ActualBorders, MoveFocusDirection.Down);
            if (nextElement == null) return;
            float posY = (float)((nextElement.ActualPosition.Y + nextElement.ActualHeight) - ActualPosition.Y);
            if ((posY - _physicalScrollOffsetY) < ActualHeight)
            {
              nextElement.TrySetFocus();
            }
            else
            {
              float diff = nextElement.ActualPosition.Y - focusedElement.ActualPosition.Y;
              if (_physicalScrollOffsetY + diff > offsetEnd) break;
              _physicalScrollOffsetY += diff;
              nextElement.TrySetFocus();
            }
          }
        }
        //OnMouseMove((float)point.X, (float)(ActualPosition.Y + y));
      }

      return;
    }

    public void ResetScroll()
    {
      _physicalScrollOffsetY = 0;
    }

    #endregion

    #region Input handling

    public override void OnKeyPressed(ref Key key)
    {
      foreach (UIElement element in Children)
      {
        if (false == element.IsVisible) continue;
        element.OnKeyPressed(ref key);
        if (key == Key.None) return;
      }
    }

    public override void OnMouseMove(float x, float y)
    {
      if (y < ActualPosition.Y) return;
      if (y > ActualHeight + ActualPosition.Y) return;
      foreach (UIElement element in Children)
      {
        if (false == element.IsVisible) continue;
        element.OnMouseMove(x, y + _physicalScrollOffsetY);
      }
    }

    #endregion
  }
}
