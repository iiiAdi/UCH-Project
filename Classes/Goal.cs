using System;
using System.Collections.Generic;
using System.Drawing;

namespace UCH_Project.Classes
{
    public class Goal : StaticObject
    {
        public Goal(int x, int y, int width, int height) : base(x, y, width, height)
        {
            // אנו מבטלים את ההתנגשות הפיזיקלית כדי שהשחקן לא יעמוד על המטרה כמו על רצפה,
            // אלא יוכל "להיכנס" לתוך השטח שלה כדי לנצח.
            this.IsCollidable = false;
        }

        public override void Draw(Graphics g)
        {
            // נצייר את המטרה בירוק בולט כדי שתהיה מזוהה, בהמשך תוכלו להחליף לתמונה של דגל
            g.FillRectangle(Brushes.Green, X, Y, Width, Height);
        }
    }
}