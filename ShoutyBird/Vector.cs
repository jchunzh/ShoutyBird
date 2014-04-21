namespace ShoutyCopter
{
    public struct Vector
    {
        public double X { get; set; }
        public double Y { get; set; }

        public static Vector operator +(Vector a, Vector b)
        {
            return new Vector {X = a.X + b.X, Y = a.Y + b.Y};
        }
    }
}
