using UnityEngine;

namespace AniBato.Game
{
    // Flat, original paper props. Collision belongs to the shared simulation.
    internal static class PaperStageScenery
    {
        internal static readonly Color Paper = new Color(.91f, .86f, .71f);
        internal static SpriteRenderer Piece(Transform parent, string name, float x, float y, float w, float h,
            Color color, int order = -24, bool round = false, float tilt = 0)
        {
            var shadow = ProceduralSprites.Create(parent, name + " Cardboard Edge", new Vector2(w + .13f, h + .13f), new Color(.12f, .13f, .18f), round, order - 1);
            shadow.transform.localPosition = new Vector3(x + .09f, y - .09f, 0); shadow.transform.localRotation = Quaternion.Euler(0, 0, tilt);
            var border = ProceduralSprites.Create(parent, name + " Cut Edge", new Vector2(w + .07f, h + .07f), Paper, round, order);
            border.transform.localPosition = new Vector3(x, y, 0); border.transform.localRotation = Quaternion.Euler(0, 0, tilt);
            var face = ProceduralSprites.Create(parent, name, new Vector2(w, h), color, round, order + 1);
            face.transform.localPosition = new Vector3(x, y, 0); face.transform.localRotation = Quaternion.Euler(0, 0, tilt);
            return face;
        }
        internal static void Line(Transform parent, string name, float x, float y, float w, float h, Color color, int order = -20, float tilt = 0)
        {
            var s = ProceduralSprites.Create(parent, name, new Vector2(w, h), color, sortingOrder: order);
            s.transform.localPosition = new Vector3(x, y, 0); s.transform.localRotation = Quaternion.Euler(0, 0, tilt);
        }
        internal static void Build(StageData data, Transform far, Transform mid)
        {
            if (data.Theme == StageTheme.FutureCity) City(far, mid);
            else if (data.Theme == StageTheme.BoardArena) Board(far, mid);
            else Festival(far, mid);
        }
        private static void City(Transform far, Transform mid)
        {
            var blue = new Color(.16f,.25f,.39f); var light = new Color(.34f,.61f,.71f);
            for (int i = -6; i <= 6; i++)
            {
                float x = i * 5.3f, h = 5 + (i * i % 5);
                Piece(far, "Paper City Tower " + i, x, 3 + h * .5f, 3.7f, h, blue, -34, tilt: i % 2 == 0 ? 1 : -1);
                for (int j = 0; j < 3; j++) Line(far,"Electronic Window",x,4+j*1.4f,2.7f,.12f,light,-31);
                Line(far,"Tower Stand",x,0,.25f,3,new Color(.3f,.29f,.25f),-35);
            }
            Piece(mid,"Management AI Screen",0,9,9,3.6f,new Color(.12f,.22f,.32f),-25);
            Piece(mid,"Management AI Eye",0,9.2f,1.8f,1.8f,light,-22,true);
            Piece(mid,"AI Pupil",.2f,9.2f,.65f,.65f,blue,-19,true);
            for(int i=-2;i<=2;i++)Line(mid,"AI Signal",i*1.3f,7.9f,.6f,.09f,light,-18);
            for(int i=-3;i<=3;i++)
            {
                Piece(mid,"Electronic Paper Panel",i*8,3.2f,5.6f,2.1f,new Color(.17f,.29f,.39f),-27);
                Line(mid,"Data Path",i*8,3.2f,4,.07f,light,-24);
            }
        }
        private static void Board(Transform far, Transform mid)
        {
            for(int side=-1;side<=1;side+=2)
            {
                for(int level=0;level<4;level++)Piece(far,"Shogi Paper Tower",side*22,1.5f+level*2,6-level*.8f,1.5f,new Color(.37f,.32f,.4f),-34);
                Piece(mid,"Shogi Piece",side*16,6.5f,3.4f,4.7f,new Color(.62f,.46f,.26f),-26,tilt:side*5);
                Piece(mid,"Shogi Piece Crown",side*16,8.8f,2.2f,1.6f,new Color(.62f,.46f,.26f),-26,tilt:side*45);
                Line(mid,"Piece Mark",side*16,6.5f,.15f,1.7f,new Color(.25f,.2f,.22f),-22);
                Line(mid,"Piece Mark",side*16,6.9f,1.6f,.15f,new Color(.25f,.2f,.22f),-22);
            }
            Piece(far,"Six by Six Paper Board",0,3.4f,24.5f,7,new Color(.64f,.53f,.36f),-34);
            for(int row=0;row<6;row++)for(int col=0;col<6;col++)
                Line(mid,"Board Backdrop Cell "+row+"-"+col,-10+col*4,.35f+row*1.08f,3.92f,1.01f,
                    (row+col)%2==0?new Color(.54f,.45f,.33f):new Color(.59f,.5f,.37f),-27);
        }
        private static void Festival(Transform far, Transform mid)
        {
            var orange=new Color(.77f,.4f,.29f);var cream=new Color(.85f,.73f,.51f);var teal=new Color(.31f,.5f,.48f);
            for(int i=-3;i<=3;i++)
            {
                float x=i*9;
                Piece(far,"Festival Stall",x,4.5f,7.8f,6,cream,-34);
                Piece(mid,"Paper Awning",x,7.7f,8.3f,.9f,orange,-27,tilt:i%2==0?1:-1);
                for(int j=0;j<5;j++)Line(mid,"Awning Stripe",x-3.3f+j*1.65f,7.7f,.75f,.9f,Paper,-24);
                Piece(mid,"Kitchen Blackboard",x,5.5f,4,2.3f,new Color(.22f,.36f,.32f),-26);
                for(int j=0;j<3;j++)Line(mid,"Chalk Menu",x,6-j*.45f,2.6f-j*.4f,.06f,cream,-23);
                Piece(mid,"Cooking Counter",x,2,8.2f,.7f,teal,-26);
                Piece(mid,"Cooking Pot",x-2.3f,2.9f,1.6f,.9f,new Color(.5f,.57f,.61f),-23,true);
                Line(mid,"Pot Lid",x-2.3f,3.4f,1.8f,.12f,Paper,-20);
                Piece(mid,"Food Plate",x+2.2f,2.55f,2.2f,.2f,Paper,-23,true);
                Piece(mid,"Festival Food",x+2.2f,2.9f,1.4f,.5f,orange,-20,true);
            }
            Line(far,"Festival String",0,10.6f,70,.07f,Paper,-31);
            for(int i=-10;i<=10;i++)Piece(far,"Paper Festival Flag",i*3,10.1f,.95f,.9f,i%2==0?teal:orange,-29,tilt:i%2==0?10:-8);
        }
    }
}

