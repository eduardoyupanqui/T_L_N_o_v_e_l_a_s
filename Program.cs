using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace T_L_N_o_v_e_l_a_s.net
{
    class Program
    {
        static async Task Main(string[] args)
        {

            Console.WriteLine("Hello World!");
            var listaFinal = new Dictionary<string, string>();
            for (int i = 119; i <= 120; i++)
            {
                var _ = string.Empty;//Para que no encuentren el repo
                var urlNovela = $"https://t{_}l{_}n{_}o{_}v{_}e{_}l{_}a{_}s.net/ver/el-otro-lado-del-paraiso-capitulo-{i}/";
                var (realUrlVideoXD, rutaVideom3u8) = await ObtenerRutasVideo(urlNovela);
                listaFinal.Add($"{i}.mp4", realUrlVideoXD);
            }
            //Imprimir Resultados
            // Para descargar con realUrlVideoXD mp4
            // ffmpeg  \
            //      -i "https://cdn-s10.cfeucdn.com/flv/api/files/videos/2019/07/24/15639804817w4h2.mp4" \
            //      -c copy 110.mp4
            // Para descargar con rutaVideom3u8
            // ffmpeg  \
            //     -headers "DNT: 1" \
            // 	   -user_agent "Mozilla/5.0 (iPad; CPU OS 11_0 like Mac OS X) AppleWebKit/604.1.34 (KHTML, like Gecko) Version/11.0 Mobile/15A5341f Safari/604.1" \
            //     -headers "Referer: https://hqq.tv/player/embed_player.php?vid=aUkJ9QL6KkDA&need_captcha=1&embed_from=&secure=0&pop=0&http_referer=https%253A%252F%252Ft_ln_ovelas.net%252F&g-recaptcha-response=03AGdBq25Hcn8lTa-_7kMMBqVRNsjgBVvD_I047UvG3ZBjKq61rzneoAtg1e1Ja2nCQPG2Bwgck7CWbN9M0mN5AqjNBi2hgF9wt9NUVbNIofAksJpFwbV59wZZ6_d7vdC62_HDgPifXP3k6lAUqIlr6BTGjx-tEyHTWA3KhSmjpUfESSndpaAH5RuMGgXqJNHcqyvxAibYO0j4NsBGXhDKqCQD3Bq3K1CC8hoahBUf7mZvGkafORrqSLaSr9RTQcRKX3a7vDMSrxX4vPA-mWb01wvSRZs9jPHaqeMuUrMupCq2T6dyxAoEwRHvzy2IyW9mHdGzJQSc-z4HkFZtLaNJORTQhDlFI5WswkViM0MXn0pJ5M8YelK6BWviPvTq5fr6qFPL0UWJyjJImLHH-o2TfGTrNnNJJdancLazkJlvtbLyYjog1V8B92aBWnc56jFr1IRMSXjXlD35" \
            //     -i "https://hgt5e3.cfeucdn.com/secip/5705/aQk0RAvmeGyoWpRyRaOTjw/MTkwLjQwLjE4NC4xNjY=/1603098000/hls-vod-s10/flv/api/files/videos/2019/07/19/1563551364540ev.mp4.m3u8" \
            //     -c copy 108.mkv
            //listaFinal.ToList().ForEach(x => Console.WriteLine($"{x.Key} - {x.Value}"));
            listaFinal.ToList().ForEach(x => Console.WriteLine($"ffmpeg  -i \"{x.Value}\" -c copy {x.Key}"));

        }

        private static async Task<(string realUrlVideoXD, string rutaVideom3u8)> ObtenerRutasVideo(string urlSite)
        {
            var realUrlVideoXD = string.Empty;
            var rutaVideom3u8 = string.Empty;
            var servidor = string.Empty;
            var nombreArchivo = string.Empty;
            try
            {
                Console.WriteLine(@"Leyendo desde {url}");

                var srcIframe = ObtenerRutaDeIframe(urlSite);
                Console.WriteLine($"Se ha leido la ruta del Iframe{srcIframe}");
                //Ejemplo srcIframe = "https://hqq.tv/e/T5IJ1HMNhuyl"
                var urlImagen = await ObtenerUrlImagen(srcIframe);
                Console.WriteLine($"La ruta de la imagen es {urlImagen}");
                //Armar su ruta de su CDN para bajar los fragmentos del video
                //Ejemplo https://hgt5e3.cfeucdn.com/secip/5705/aQk0RAvmeGyoWpRyRaOTjw/MTkwLjQwLjE4NC4xNjY=/1603098000/hls-vod-s10/flv/api/files/videos/2019/07/24/15639215044r0cv.mp4.m3u8
                (realUrlVideoXD, servidor, nombreArchivo) = ObtenerInformacionUrlImagenTupla(urlImagen);
                rutaVideom3u8 = $"https://hgt5e3.cfeucdn.com/secip/5705/aQk0RAvmeGyoWpRyRaOTjw/MTkwLjQwLjE4NC4xNjY=/1603098000/hls-vod-{servidor}/flv/api/files/videos/{nombreArchivo}.mp4.m3u8";
                Console.WriteLine($"La ruta completa del video mp4 es {realUrlVideoXD}");
                Console.WriteLine($"La ruta completa del video m3u8 es {rutaVideom3u8}");
                Console.WriteLine("Fin");

            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"Uppss algo salió mal: {ex.Message}");
            }
            return (realUrlVideoXD, rutaVideom3u8);
        }

        public static string ObtenerRutaDeIframe(string urlSite)
        {
            WebClient x = new WebClient();
            string pageSource = x.DownloadString(urlSite);
            var groups = Regex.Match(pageSource, @"\<iframe\b[^>]*\>\s*(?<iframe>[\s\S]*?)\</iframe\>", RegexOptions.IgnoreCase);
            var tagIframe = groups.Value;
            //Console.WriteLine(tagIframe);
            Regex regex = new Regex(@"(?<=\bsrc="")[^""]*");
            Match match = regex.Match(tagIframe);
            if (match == null || string.IsNullOrEmpty(match.Value)) throw new Exception("No se ha encontrado el iframe :(");
            return match.Value;
        }
        public static async Task<string> ObtenerUrlImagen(string urlIFrame)
        {
            HtmlWeb client = new HtmlWeb();
            var html = await client.LoadFromWebAsync(urlIFrame);
            var metaTags = html.DocumentNode.SelectNodes("//meta");
            var urlImagen = GetUrlImagen(metaTags);
            if (string.IsNullOrEmpty(urlImagen)) throw new Exception("No se ha encontrado la ruta de la imagen :(");
            return urlImagen;
        }

        public static string GetUrlImagen(HtmlNodeCollection metaTags)
        {
            foreach (var tag in metaTags)
            {
                var content = tag.Attributes["content"];
                var property = tag.Attributes["property"];
                if (property != null)
                {
                    switch (property.Value.ToLower())
                    {
                        case "og:image":
                            return content.Value;
                        default:
                            break;
                    }
                }
            }
            return String.Empty;
        }

        public static (string realUrlVideoXD, string servidor, string nombreArchivo) ObtenerInformacionUrlImagenTupla(string rutaImagen)
        {
            //Ejemplo https://cdn-s10.cfeucdn.com/flv/api/files/thumbs/2019/07/02/1562036752l099e-640x480-1.jpg
            //Ejemplo https://cdn-s10.cfeucdn.com/flv/api/files/videos/2019/08/07/1565146172vke9v.mp4
            string servidor = string.Empty;
            string nombreArchivo = string.Empty;
            servidor = GetStringBetween(rutaImagen, "https://cdn-", ".cfeucdn.com");//"s10"
            nombreArchivo = GetStringBetween(rutaImagen, "thumbs/", "-640x480-1.jpg");//"2019/07/24/15639215044r0cv"
            if (string.IsNullOrEmpty(servidor) || string.IsNullOrEmpty(nombreArchivo))
            {
                throw new Exception("No se logro obtener Info de la ruta de Imagen :(");
            }
            string realUrlVideoXD = $"https://cdn-{servidor}.cfeucdn.com/flv/api/files/videos/{nombreArchivo}.mp4";
            return (realUrlVideoXD, servidor, nombreArchivo);
        }

        private static string GetStringBetween(string message, string v1, string v2)
        {
            //var cadena = "https://cdn-s10.cfeucdn.com";
            //Console.WriteLine(GetStringBetween(cadena,"https://cdn-",".cfeucdn.com"));
            if (string.IsNullOrEmpty(message))
            {
                return string.Empty;
            }
            var St = message.ToString();
            int pFrom = St.IndexOf(v1) + v1.Length;
            int pTo = St.LastIndexOf(v2);
            return St.Substring(pFrom, pTo - pFrom);
        }
    }
}
