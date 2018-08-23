using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;
using SFML.Window;

namespace Zevruk
{
    internal abstract class Setting
    {
        protected Vector2f position;
        protected Font font = Fonts.Verdana;
        protected Text titleText;

        public Setting(string title, Vector2f position)
        {
            this.Title = title;
            this.position = position;
            this.titleText = new Text(title, this.font, 40)
            {
                Position = position
            };
        }

        public string Title { get; set; }
        public abstract string Value { get; }

        public virtual void Draw(RenderWindow window)
        {
            window.Draw(this.titleText);
        }
    }

    internal class ValueSetting : Setting
    {
        protected Texture arrowTexture;
        protected Sprite leftArrow, rightArrow;
        protected IList<string> values;
        protected int active;

        public ValueSetting(string title, IList<string> values, int activeIndex, Vector2f position) : base(title, position)
        {
            if (activeIndex < 0 || activeIndex >= values.Count)
                throw new ArgumentOutOfRangeException(nameof(activeIndex));

            this.arrowTexture = new Texture("images/settingArrow.png");
            this.leftArrow = new Sprite(this.arrowTexture);
            this.rightArrow = new Sprite(this.arrowTexture)
            {
                Scale = new Vector2f(-1, 1)
            };
            this.values = values;
            this.active = activeIndex;
        }

        public override string Value
        {
            get => this.values[this.active];
        }

        public override void Draw(RenderWindow window)
        {
            base.Draw(window);
        }
    }

    internal class BoolSetting : Setting
    {
        protected bool value;
        protected Texture tickTexture;
        protected Sprite tick;
        protected RectangleShape frame;

        public BoolSetting(bool value, string title, Vector2f position) : base(title, position)
        {
            this.value = value;
            Image image = new Image("images/yes.png");
            image.CreateMaskFromColor(Color.White);
            this.tickTexture = new Texture(image);
            this.tick = new Sprite(this.tickTexture)
            {
                Position = new Vector2f(this.position.X + 40, this.position.Y)
            };
            this.frame = new RectangleShape(new Vector2f(40, 40))
            {
                FillColor = Color.Transparent,
                OutlineColor = Color.Black,
                Position = new Vector2f(this.position.X + 40, this.position.Y)
            };
        }

        public override string Value => this.value.ToString();

        public override void Draw(RenderWindow window)
        {
            base.Draw(window);
            window.Draw(this.frame);
            if (this.value)
                window.Draw(this.tick);
        }
    }
}
