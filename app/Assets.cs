using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Web.Script.Serialization;

namespace Judge
{
    public sealed class FrameInfo
    {
        public string file,sha256;public int width,height;public int[] bounds;
        public double scale,supportX,supportY,bodyAnchorX;public bool draft,hasBodyAnchor;
        public Rectangle Bounds{get{return Rectangle.FromLTRB(bounds[0],bounds[1],bounds[2],bounds[3]);}}
    }
    public sealed class ClipInfo
    {
        public string[] frames;public double[] seconds;public string pose;public bool draft;public double peakHoldMin,peakHoldMax;
        public double Duration{get{double result=0;foreach(double v in seconds)result+=v;return result;}}
        public string At(double time){for(int i=0;i<frames.Length;i++){if(time<seconds[i])return frames[i];time-=seconds[i];}return frames[frames.Length-1];}
        public double DurationWithHold(double hold){return Duration+(peakHoldMax>0?hold-seconds[1]:0);}
        public string At(double time,double hold)
        {if(peakHoldMax<=0)return At(time);if(time<seconds[0])return frames[0];if(time<seconds[0]+hold)return frames[1];return frames[frames.Length-1];}
    }
    public sealed class ArtManifest{public Dictionary<string,FrameInfo> frames;public Dictionary<string,ClipInfo> clips;}
    internal sealed class Assets:IDisposable
    {
        public readonly ArtManifest Manifest;
        readonly Dictionary<string,Bitmap> cache=new Dictionary<string,Bitmap>();readonly List<string> order=new List<string>();
        internal const int CacheLimit=6;
        public Assets()
        {
            using(Stream stream=Open("Judge.Manifest"))using(StreamReader reader=new StreamReader(stream))
                Manifest=new JavaScriptSerializer().Deserialize<ArtManifest>(reader.ReadToEnd());
        }
        internal static Stream Open(string resource)
        {Stream result=Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);if(result==null)throw new InvalidDataException("Missing embedded resource: "+resource);return result;}
        public Bitmap Get(string key)
        {
            Bitmap result;
            if(cache.TryGetValue(key,out result)){order.Remove(key);order.Add(key);return result;}
            using(Stream stream=Open("Judge.Art."+key))using(Bitmap original=new Bitmap(stream))result=(Bitmap)original.Clone();
            if(result.Width!=Manifest.frames[key].width||result.Height!=Manifest.frames[key].height){result.Dispose();throw new InvalidDataException("Artwork canvas mismatch: "+key);}
            if(order.Count==CacheLimit){string remove=order[0];order.RemoveAt(0);cache[remove].Dispose();cache.Remove(remove);}
            cache.Add(key,result);order.Add(key);return result;
        }
        public void Dispose(){foreach(Bitmap bitmap in cache.Values)bitmap.Dispose();cache.Clear();order.Clear();}
    }
}
