using System;

namespace LibMMD.Material
{
    public class MMDTexture
    {
        public string TexturePath { get; set; }
        public bool IsGlobalToon;
        public MMDTexture(string texturePath)
        {
            TexturePath = texturePath;
        }

        protected bool Equals(MMDTexture other)
        {
            return string.Equals(TexturePath, other.TexturePath);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MMDTexture) obj);
        }

        public override int GetHashCode()
        {
            return TexturePath != null ? TexturePath.GetHashCode() : 0;
        }

        private MMDTexture()
        {
            
        }
     
    }
}