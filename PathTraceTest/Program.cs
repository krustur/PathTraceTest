using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PathTraceTest
{
    static class Program
    {
        public static Random Random;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            var fileName = $"output_{DateTime.Now.ToShortDateString()}_{DateTime.Now.ToShortTimeString()}";
            fileName = fileName.Replace(":", "");
            fileName = fileName.Replace("-", "");

            int nx = 600;
            int ny = 200;
            var ns = 25;

            var stdcout = new StringBuilder();
            stdcout.AppendLine("P3");
            stdcout.Append(nx);
            stdcout.Append(" ");
            stdcout.Append(ny);
            stdcout.AppendLine();
            stdcout.AppendLine("255");
            Bitmap bmp = new Bitmap(nx, ny);



            float R = (float) Math.Cos(Math.PI / 4);
            //var world = new HitableList(
            //    new List<Hitable>
            //    {
            //        new Sphere(new Vector3(0f, 0f, -1.0f), 0.5f, new Lambertian(new Vector3(0.1f, 0.2f, 0.5f))),
            //        new Sphere(new Vector3(0f, -100.5f, -1.0f), 100f, new Lambertian(new Vector3(0.8f, 0.8f, 0.0f))),
            //        new Sphere(new Vector3(1f, 0f, -1f), 0.5f, new Metal(new Vector3(0.8f, 0.6f, 0.2f), 0.3f)),
            //        new Sphere(new Vector3(-1f, 0f, -1f), 0.5f, new Dielectric(1.5f)),
            //        new Sphere(new Vector3(-1f, 0f, -1f), -0.45f, new Dielectric(1.5f)),

            //        //new Sphere(new Vector3(-R, 0f, -1.0f), R, new Lambertian(new Vector3(0f, 0f, 1f))),
            //        //new Sphere(new Vector3(R, 0f, -1.0f), R, new Lambertian(new Vector3(1f, 0f, 0f))),
            //    }
            //    );
            var world = new HitableList(RandomScene());
            var aspect = (float)nx / (float)ny;
            //var lookFrom = new Vector3(3, 3, 2);
            //var lookAt = new Vector3(0, 0, -1);
            //float distToFocus = (lookAt - lookFrom).Length();
            //float aperture = 2f;


            var lookFrom = new Vector3(13,2,3);
            var lookAt = new Vector3(0,0,0);
            float distToFocus = 10.0f;
            float aperture = 0.1f;

            //list.Add(new Sphere(new Vector3(0, 1, 0), 1f, new Dielectric(1.5f)));
            //list.Add(new Sphere(new Vector3(-4, 1, 0), 1f, new Lambertian(new Vector3(0.4f, 0.2f, 0.1f))));
            //list.Add(new Sphere(new Vector3(4, 1, 0), 1f, new Metal(new Vector3(0.7f, 0.6f, 0.5f), 0f)));


            var camera = new Camera(lookFrom, lookAt, new Vector3(0, 1, 0), 20, aspect, aperture, distToFocus);

            Random = new Random();
            //var imageData = new byte[3*nx*ny];
            for (int j = ny - 1; j >= 0; j--)
            {
                for (int i = 0; i < nx; i++)
                {
                    Vector3 col = new Vector3();
                    for (int s = 0; s < ns; s++)
                    {
                        float u = ((float) i + (float)Random.NextDouble()) / nx;
                        float v = ((float) j + (float)Random.NextDouble()) / ny;
                        //var p = r.PointAtParameter(2.0f);
                        var r = camera.GetRay(u, v);
                        col += Color(r, world, 0);
                    }

                    col = col / ns;
                    col = new Vector3((float) Math.Sqrt(col.X), (float) Math.Sqrt(col.Y), (float) Math.Sqrt(col.Z));
                    int ir =  (int) (255f * col.X);
                    int ig =  (int) (255f * col.Y);
                    int ib =  (int) (255f * col.Z);
                    stdcout.Append(ir);
                    stdcout.Append(" ");
                    stdcout.Append(ig);
                    stdcout.Append(" ");
                    stdcout.Append(ib);
                    stdcout.AppendLine();
                    bmp.SetPixel(i, ny-1-j, System.Drawing.Color.FromArgb(ir, ig, ib));
                }
                Debug.WriteLine($"y: {ny-1-j}/{ny}");
            }

            //using (var ms = new MemoryStream(imageData))
            //{
                //bmp = new Bitmap(ms);
                //var Image = new Image();
            //}
            File.WriteAllText($@"d:\github\PathTraceTest\{fileName}.ppm", stdcout.ToString());
            bmp.Save($@"d:\github\PathTraceTest\{fileName}.bmp");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1(bmp));
        }

        private static IList<Hitable> RandomScene()
        {
            var rand = new Random();
            //int n = 500;
            var list = new List<Hitable>();
            list.Add(new Sphere(new Vector3(0, -1000, 0), 1000, new Lambertian(new Vector3(0.5f, 0.5f, 0.5f))));
            //int i = 1;
            for (int a = -11; a < 11; a++)
            {
                for (int b = -11; b < 11; b++)
                {
                    var chooseMat = rand.NextDouble();
                    var center = new Vector3((float) (a+0.9f * rand.NextDouble()), 0.2f, (float) (b+0.9f*rand.NextDouble()));
                    if ((center - new Vector3(4, 0.2f, 0)).Length() > 0.9f)
                    {
                        Material material;
                        if (chooseMat < 0.50f)
                        {
                            material = new Lambertian(new Vector3((float) (rand.NextDouble() * rand.NextDouble()),
                                (float) (rand.NextDouble() * rand.NextDouble()),
                                (float) (rand.NextDouble() * rand.NextDouble())));
                        }
                        else if (chooseMat < 0.75f)
                        {
                            material = new Metal(
                                new Vector3(
                                    (float) (0.5f * (1 + rand.NextDouble())),
                                    (float) (0.5f * (1 + rand.NextDouble())), 
                                    (float) (0.5f * (1 + rand.NextDouble()))),
                                (float) (0.5f * rand.NextDouble()));
                        }
                        else
                        {
                            material = new Dielectric(1.5f);
                        }
                        var sphere = new Sphere(center, 0.2f, material);
                        list.Add(sphere);

                    }
                }

            }

            list.Add(new Sphere(new Vector3(0,1,0),1f, new Dielectric(1.5f)));
            list.Add(new Sphere(new Vector3(-4,1,0),1f, new Lambertian(new Vector3(0.4f, 0.2f, 0.1f))));
            list.Add(new Sphere(new Vector3(4,1,0),1f, new Metal(new Vector3(0.7f, 0.6f, 0.5f), 0f)));
            return list;
        }

        private static Vector3 Color(Ray ray, Hitable world, int depth)
        {
            var rec = new HitRecord();
            if (world.Hit(ray, 0.001f, float.MaxValue, ref rec))
            {
                Ray scattered;
                Vector3 attenuation;
                if (depth < 5 && rec.Material.Scatter(ray, rec, out attenuation, out scattered))
                {
                    return attenuation * Color(scattered, world, depth + 1);
                }
                else
                {
                    return Vector3.Zero;
                    //return new Vector3(0, 0, 0);
                }
                //var target = rec.P + rec.Normal + RandomInUnitSphere();
                ////return 0.5f * new Vector3(rec.Normal.X + 1f, rec.Normal.Y + 1, rec.Normal.Z + 1);
                //return 0.5f * Color(new Ray(rec.P, target - rec.P), world);
            }
            else
            {
                var unitDirection = Vector3.Normalize(ray.Direction);
                var t = 0.5f * (unitDirection.Y + 1.0f);
                var result = (1.0f - t) * new Vector3(1.0f, 1.0f, 1.0f) + t * new Vector3(0.5f, 0.7f, 1.0f);
                return result;
            }
        }

        public static Vector3 RandomInUnitSphere()
        {
            Vector3 p;
            do
            {
                p = (2.0f * new Vector3((float) Random.NextDouble(), (float) Random.NextDouble(), (float) Random.NextDouble())) - Vector3.One;
            } while (p.LengthSquared() >= 1.0f);

            return p;
        }

        public static Vector3 RandomInUnitDisk()
        {
            Vector3 p;
            do
            {
                p = (2.0f * new Vector3((float)Random.NextDouble(), (float)Random.NextDouble(), 0)) - new Vector3(1,1,0);
            } while (Vector3.Dot(p, p) >= 1.0f);

            return p;
        }

        public static Vector3 Reflect(Vector3 v, Vector3 n)
        {
            return v - 2 * Vector3.Dot(v, n) * n;
        }

        public static bool Refract(Vector3 v, Vector3 n, float niOverNt, out Vector3 refracted)
        {
            var uv = Vector3.Normalize(v);
            float dt = Vector3.Dot(uv, n);
            float discriminant = 1.0f - niOverNt * niOverNt * (1 - dt * dt);
            if (discriminant > 0)
            {
                refracted = niOverNt * (uv - n * dt) - n * (float) Math.Sqrt(discriminant);
                return true;
            }
            else
            {
                refracted = Vector3.One; //Hack!
                return false;
            }
        }

        public static float Schlick(float cosine, float refIdx)
        {
            float r0 = (1f - refIdx) / (1f + refIdx);
            r0 = r0 * r0;
            r0 =  (r0 + (1f - r0) * (float)Math.Pow((1f - cosine), 5f));
            return r0;
        }
    }

    public class Ray
    {
        public Vector3 Origin { get; }
        public Vector3 Direction { get; }

        public Ray(Vector3 origin, Vector3 direction)
        {
            Origin = origin;
            Direction = direction;
        }

        public Vector3 PointAtParameter(float t)
        {
            var result = Origin + t * Direction;
            return result;
        }
    }

    public struct HitRecord
    {
        public float T;
        public Vector3 P;
        public Vector3 Normal { get; set; }
        public Material Material { get; set; }

    }

    public abstract class Hitable
    {
        public abstract bool Hit(Ray ray, float t_min, float t_max, ref HitRecord rec);
    }

    public class HitableList : Hitable
    {
        public IList<Hitable> List { get; }

        public HitableList(IList<Hitable> list)
        {
            List = list;
        }

        public override bool Hit(Ray ray, float t_min, float t_max, ref HitRecord rec)
        {
            HitRecord tempRec = new HitRecord();

            var hitAnything = false;
            float closestSoFar = t_max;
            foreach (var hitable in List)
            {
                if (hitable.Hit(ray, t_min, closestSoFar, ref tempRec))
                {
                    hitAnything = true;
                    closestSoFar = tempRec.T;
                    rec = tempRec;
                }
            }
            return hitAnything;
        }
    }

    public class Sphere : Hitable
    {
        public Vector3 Center { get; }
        public float Radius { get; }
        public Material Material { get; }

        public Sphere(Vector3 center, float radius, Material material)
        {
            Center = center;
            Radius = radius;
            Material = material;
        }

        public override bool Hit(Ray ray, float t_min, float t_max, ref HitRecord rec)
        {
            var oc = ray.Origin - Center;
            var a = Vector3.Dot(ray.Direction, ray.Direction);
            var b = Vector3.Dot(oc, ray.Direction);
            var c = Vector3.Dot(oc, oc) - Radius * Radius;
            var discriminant = b * b - a * c;
            if (discriminant > 0)
            {
                var temp = (float)((-b - Math.Sqrt(b*b-a*c)) / a);
                if (temp < t_max && temp > t_min)
                {
                    rec.T = temp;
                    rec.P = ray.PointAtParameter(rec.T);
                    rec.Normal = (rec.P - Center) / Radius;
                    rec.Material = Material;
                    return true;
                }
                temp = (float)((-b + Math.Sqrt(b * b - a * c)) / a);
                if (temp < t_max && temp > t_min)
                {
                    rec.T = temp;
                    rec.P = ray.PointAtParameter(rec.T);
                    rec.Normal = (rec.P - Center) / Radius;
                    rec.Material = Material;
                    return true;
                }
            }

            return false;
        }

    }

    public class Camera
    {
        private readonly Vector3 _lowerLeftCorner;// = new Vector3(-2.0f, -1.0f, -1.0f);
        private readonly Vector3 _horizontal;
        private readonly Vector3 _vertical;
        private readonly Vector3 _origin;
        private float _lensRadius;
        private Vector3 _w;
        private Vector3 _u;
        private Vector3 _v;

        public Camera(Vector3 lookFrom, Vector3 lookAt, Vector3 up, float vfov, float aspect, float aperture, float focusDist)
        {
            _lensRadius = aperture / 2f;
            float theta = (float) (vfov * Math.PI / 180f);
            float halfHeigth = (float) Math.Tan(theta / 2);
            float  halfWidth = aspect * halfHeigth;
            _origin = lookFrom;
            _w = Vector3.Normalize(lookFrom - lookAt);
            _u = Vector3.Normalize(Vector3.Cross(up, _w));
            _v = Vector3.Cross(_w, _u);
            //_lowerLeftCorner = new Vector3(-halfWidth, -halfHeigth, -1.0f);
            _lowerLeftCorner = _origin - (halfWidth*focusDist*_u) -(halfHeigth*focusDist*_v) - focusDist*_w;
            _horizontal = 2 * halfWidth * focusDist * _u;
            _vertical = 2 * halfHeigth * focusDist * _v;
        }
        public Ray GetRay(float s, float t)
        {
            var rd = _lensRadius * Program.RandomInUnitDisk();
            var offset = _u * rd.X + _v * rd.Y;
            var r = new Ray(_origin + offset, _lowerLeftCorner + s * _horizontal + t * _vertical - _origin - offset);
            return r;
        }
    }

    public abstract class Material
    {
        public abstract bool Scatter(Ray rayIn, HitRecord rec, out Vector3 attenuation, out Ray scattered);
    }

    public class Lambertian: Material
    {
        private readonly Vector3 _albedo;

        public Lambertian(Vector3 albedo)
        {
            _albedo = albedo;
        }

        public override bool Scatter(Ray rayIn, HitRecord rec, out Vector3 attenuation, out Ray scattered)
        {
            var target = rec.P + rec.Normal + Program.RandomInUnitSphere();
            scattered = new Ray(rec.P, target - rec.P);
            attenuation = _albedo;
            return true;
        }
    }

    public class Metal : Material
    {
        private readonly Vector3 _albedo;
        private float _fuzz;

        public Metal(Vector3 albedo, float f)
        {
            _albedo = albedo;
            if (f < 1)
            {
                _fuzz = f;
            }
            else
            {
                _fuzz = 1f;
            }
        }

        public override bool Scatter(Ray rayIn, HitRecord rec, out Vector3 attenuation, out Ray scattered)
        {
            var reflected = Program.Reflect(Vector3.Normalize(rayIn.Direction), rec.Normal);
            scattered = new Ray(rec.P, reflected + _fuzz*Program.RandomInUnitSphere());
            attenuation = _albedo;
            return (Vector3.Dot(scattered.Direction, rec.Normal) > 0f);
        }
    }

    public class Dielectric : Material
    {
        private readonly float _refIdx;

        public Dielectric(float ri)
        {
            _refIdx = ri;
        }

        public override bool Scatter(Ray rayIn, HitRecord rec, out Vector3 attenuation, out Ray scattered)
        {
            Vector3 outwardNormal;
            var reflected = Program.Reflect(Vector3.Normalize(rayIn.Direction), rec.Normal);
            float niOverNt;
            attenuation = new Vector3(1, 1, 1);
            Vector3 refracted;
            float reflectProb;
            float cosine;
            if (Vector3.Dot(rayIn.Direction, rec.Normal) > 0)
            {
                outwardNormal = -rec.Normal;
                niOverNt = _refIdx;
                cosine = _refIdx * Vector3.Dot(rayIn.Direction, rec.Normal) / rayIn.Direction.Length();
            }
            else
            {
                outwardNormal = rec.Normal;
                niOverNt = 1.0f / _refIdx;
                cosine = -(Vector3.Dot(rayIn.Direction, rec.Normal) / rayIn.Direction.Length());
            }

            if (Program.Refract(rayIn.Direction, outwardNormal, niOverNt, out refracted))
            {
                reflectProb = Program.Schlick(cosine, _refIdx);
                //scattered = new Ray(rec.P, refracted);
            }
            else
            {
                //todo: remove line below!?
                //scattered = new Ray(rec.P, reflected);
                reflectProb = 1.0f;
            }

            if (Program.Random.NextDouble() < reflectProb)
            {
                scattered = new Ray(rec.P, reflected);
            }
            else
            {
                scattered = new Ray(rec.P, refracted);
            }
            return true;
        }
    }

}
