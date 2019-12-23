using System.Collections.Generic;
using System.Linq;

namespace TalosDownpatcher {
  public class SteamManifest {
    public readonly int appId;
    public readonly int depotId;
    public readonly long manifestId;

    public readonly int numFiles; // @Cleanup: Unused, but definitely should be (at least for validation).
    public readonly long size;
    public readonly Package package;
    public SteamManifest(int appId, int depotId, long manifestId, int numFiles, long size, Package package) {
      this.appId = appId;
      this.depotId = depotId;
      this.manifestId = manifestId;
      this.numFiles = numFiles;
      this.size = size;
      this.package = package;
    }
  }

  public static class ManifestData {
    public static readonly List<int> commonVersions = new List<int> {
      440323, 326589, 301136, 244371
    };
    public static readonly List<int> allVersions = new List<int> {
      440323, 429074, 426014, 424910, 326589, 301136, 300763, 293384,
      291145, 284152, 277544, 269335, 267252, 264510, 260924, 258375,
      252786, 250756, 249913, 249740, 248828, 248139, 246379, 244371,
      243520, 226087, 224995, 224531, 223249, 222477, 221394, 220996,
      220675, 220625, 220480
    };
    // Possibly replace with:
    // public static List<int> allVersions { get { return data.Keys.ToList(); } }

    public static long GetDownloadSize(int version, Package package = Package.All) {
      long totalDownloadSize = 0;
      foreach (var manifest in GetManifestsForVersion(version, package)) {
        totalDownloadSize += manifest.size;
      }
      return totalDownloadSize;
    }

    public static IEnumerable<SteamManifest> GetManifestsForVersion(int version, Package package = Package.All) {
      return data[version].Where(m => package == Package.All || package == m.package);
    }

    private static readonly Dictionary<int, List<SteamManifest>> data = InitData();
    private static Dictionary<int, List<SteamManifest>> InitData() {
      Dictionary<int, List<SteamManifest>> data = new Dictionary<int, List<SteamManifest>>();

      data[220480] = new List<SteamManifest>() {
        new SteamManifest(257510, 257511, 4122334240058475272, 2, 48908496, Package.Main),
        new SteamManifest(257510, 257515, 1348826399794024471, 32, 5055625401, Package.Main),
        new SteamManifest(257510, 257516, 1237958166729860756, 6, 117245, Package.Main),
        new SteamManifest(257510, 257519, 2322040020544521685, 1, 1739595, Package.Main),
        new SteamManifest(257510, 322022, 2824340380872620137, 3, 172228256, Package.Prototype)
      };

      data[220625] = new List<SteamManifest> {
        new SteamManifest(257510, 257511, 8087374737096894864,  2,   48901328, Package.Main),
        new SteamManifest(257510, 257515, 3617089317964903627, 32, 5217247608, Package.Main),
        new SteamManifest(257510, 257516, 1237958166729860756,  6,     117245, Package.Main),
        new SteamManifest(257510, 257519, 1272083368566319066,  1,    1789447, Package.Main),
        new SteamManifest(257510, 322022, 8183524469048336858,  3,  206907989, Package.Prototype)
      };

      data[220675] = new List<SteamManifest> {
        new SteamManifest(257510, 257511, 2018192542376932921,  2,   48900816, Package.Main),
        new SteamManifest(257510, 257515, 8986158524600611840, 32, 5217247413, Package.Main),
        new SteamManifest(257510, 257516, 1237958166729860756,  6,     117245, Package.Main),
        new SteamManifest(257510, 257519, 4554990112195176406,  1,    1789447, Package.Main),
        new SteamManifest(257510, 322022, 4383443687812477256,  3,  206909511, Package.Prototype)
      };

      data[220996] = new List<SteamManifest> {
        new SteamManifest(257510, 257511, 3475103749484918333,  2,   48912080, Package.Main),
        new SteamManifest(257510, 257515, 0415067456888111848, 27, 5439377389, Package.Main),
        new SteamManifest(257510, 257516, 1237958166729860756,  6,     117245, Package.Main),
        new SteamManifest(257510, 257519, 4906101680219337427,  1,    1858219, Package.Main),
        new SteamManifest(257510, 322022, 7673940384630485679,  3,  211726262, Package.Prototype)
      };

      data[221394] = new List<SteamManifest> {
        new SteamManifest(257510, 257511, 1222567553988779631,  2,   48947920, Package.Main),
        new SteamManifest(257510, 257515, 7204783264154690101, 27, 5842288583, Package.Main),
        new SteamManifest(257510, 257516, 1237958166729860756,  6,     117245, Package.Main),
        new SteamManifest(257510, 257519, 6019585236142413402,  1,    1981411, Package.Main),
        new SteamManifest(257510, 322022, 6888774931510513164,  3,  218106434, Package.Prototype)
      };

      data[222477] = new List<SteamManifest> {
        new SteamManifest(257510, 257511, 0918811395787437543,  2,   49005264, Package.Main),
        new SteamManifest(257510, 257515, 6267514976170907571, 27, 6094446568, Package.Main),
        new SteamManifest(257510, 257516, 1237958166729860756,  6,     117245, Package.Main),
        new SteamManifest(257510, 257519, 6671071736059004158,  1,    2059976, Package.Main),
        new SteamManifest(257510, 322022, 8917339509592123029,  3,  225475410, Package.Prototype)
      };

      data[223249] = new List<SteamManifest> {
        new SteamManifest(257510, 257511, 4657839721186080333,  2,   49014480, Package.Main),
        new SteamManifest(257510, 257515, 8304752583573271963, 27, 6094446741, Package.Main),
        new SteamManifest(257510, 257516, 1237958166729860756,  6,     117245, Package.Main),
        new SteamManifest(257510, 257519, 7835318215284690246,  1,    2059976, Package.Main),
        new SteamManifest(257510, 322022, 3867407920981508845,  3,  225475676, Package.Prototype)
      };

      data[224531] = new List<SteamManifest> {
        new SteamManifest(257510, 257511, 3207947001105705384,  2,   37991120, Package.Main),
        new SteamManifest(257510, 257515, 5798939509479840711, 28, 6281382910, Package.Main),
        new SteamManifest(257510, 257516, 1237958166729860756,  6,     117245, Package.Main),
        new SteamManifest(257510, 257519, 3565289474630779855,  1,    2117568, Package.Main),
        new SteamManifest(257510, 322022, 7516537008051594086,  3,  243488546, Package.Prototype)
      };

      data[224995] = new List<SteamManifest> {
        new SteamManifest(257510, 257511, 9130874505093356390,  2,   38000336, Package.Main),
        new SteamManifest(257510, 257515, 4840744566182207229, 28, 6281389510, Package.Main),
        new SteamManifest(257510, 257516, 1237958166729860756,  6,     117245, Package.Main),
        new SteamManifest(257510, 257519, 0660166059336687264,  1,    2117568, Package.Main),
        new SteamManifest(257510, 322022, 6536174967355028889,  3,  243487857, Package.Prototype)
      };

      data[226087] = new List<SteamManifest> {
        new SteamManifest(257510, 257511, 2501832898327740448,  2,   38003408, Package.Main),
        new SteamManifest(257510, 257515, 3826319505876650203, 29, 6282258753, Package.Main),
        new SteamManifest(257510, 257516, 1237958166729860756,  6,     117245, Package.Main),
        new SteamManifest(257510, 257519, 5081265059426016345,  1,    2117916, Package.Main),
        new SteamManifest(257510, 322022, 7180495594989737482,  4,  275966134, Package.Prototype)
      };

      data[243520] = new List<SteamManifest> {
        new SteamManifest(257510, 257511, 0486897345253495735,  2,   37835472, Package.Main),
        new SteamManifest(257510, 257515, 0762065295327824568, 29, 6870004878, Package.Main),
        new SteamManifest(257510, 257516, 7901792711762181252,  7,     218389, Package.Main),
        new SteamManifest(257510, 257519, 2020897382852338977,  1,    2312159, Package.Main),
        new SteamManifest(257510, 358470, 3645040643832281528,  2,  704403859, Package.Gehenna),
        new SteamManifest(257510, 322022, 1221230661228297401,  4,  291798721, Package.Prototype)
      };

      data[244371] = new List<SteamManifest> {
        new SteamManifest(257510, 257511, 7689461949196252397,  2,   37835472, Package.Main),
        new SteamManifest(257510, 257515, 8579671038386532826, 28, 6427365830, Package.Main),
        new SteamManifest(257510, 257516, 7901792711762181252,  7,     218389, Package.Main),
        new SteamManifest(257510, 257519, 2675083093007158979,  1,    2175966, Package.Main),
        new SteamManifest(257510, 358470, 8176613815265754262,  3,  749886317, Package.Gehenna),
        new SteamManifest(257510, 322022, 0570457731767330233,  2,  191173592, Package.Prototype)
      };

      data[246379] = new List<SteamManifest> {
        new SteamManifest(257510, 257511, 7720637873729107634,  2,   37919440, Package.Main),
        new SteamManifest(257510, 257515, 4896296030980495433, 29, 6546725617, Package.Main),
        new SteamManifest(257510, 257516, 0537534056471328674,  2,     208576, Package.Main),
        new SteamManifest(257510, 257519, 7776793304385903116,  1,    2213167, Package.Main),
        new SteamManifest(257510, 358470, 7419870507570765883,  3,  866850779, Package.Gehenna),
        new SteamManifest(257510, 322022, 4423801769448698176,  2,  191045699, Package.Prototype)
      };

      data[248139] = new List<SteamManifest> {
        new SteamManifest(257510, 257511, 1500898004646373285,  2,   37932240, Package.Main),
        new SteamManifest(257510, 257515, 8022908710052117236, 30, 6589267934, Package.Main),
        new SteamManifest(257510, 257516, 0537534056471328674,  2,     208576, Package.Main),
        new SteamManifest(257510, 257519, 4793167457039384090,  1,    2226235, Package.Main),
        new SteamManifest(257510, 358470, 9140920440213320364,  4,  896460286, Package.Gehenna),
        new SteamManifest(257510, 322022, 5515868016021140366,  3,  191084683, Package.Prototype)
      };

      data[248828] = new List<SteamManifest> {
        new SteamManifest(257510, 257511, 4259291835568024105,  2,   37933264, Package.Main),
        new SteamManifest(257510, 257515, 5103279241419561908, 30, 6589271132, Package.Main),
        new SteamManifest(257510, 257516, 0537534056471328674,  2,     208576, Package.Main),
        new SteamManifest(257510, 257519, 7130120369117951230,  1,    2226235, Package.Main),
        new SteamManifest(257510, 358470, 1215334353744988752,  4,  896460268, Package.Gehenna),
        new SteamManifest(257510, 322022, 6408748820177783708,  3,  191084683, Package.Prototype)
      };

      data[249740] = new List<SteamManifest> {
        new SteamManifest(257510, 257511, 2372465056935606724,  2,   37954768, Package.Main),
        new SteamManifest(257510, 257515, 7774619190361198089, 29, 6590576873, Package.Main),
        new SteamManifest(257510, 257516, 7546122753718189131,  2,     250448, Package.Main),
        new SteamManifest(257510, 257519, 4648185825889483487,  1,    2226635, Package.Main),
        new SteamManifest(257510, 358470, 6996102492712095516,  4,  896460342, Package.Gehenna),
        new SteamManifest(257510, 322022, 5134711953944719494,  3,  191084684, Package.Prototype)
      };

      data[249913] = new List<SteamManifest> {
        new SteamManifest(257510, 257511, 0104511117861601608,  2,   37954256, Package.Main),
        new SteamManifest(257510, 257515, 8145595254539830554, 29, 6590576890, Package.Main),
        new SteamManifest(257510, 257516, 7546122753718189131,  2,     250448, Package.Main),
        new SteamManifest(257510, 257519, 0264858112795519304,  1,    2226635, Package.Main),
        new SteamManifest(257510, 358470, 2832266850408399266,  4,  896460324, Package.Gehenna),
        new SteamManifest(257510, 322022, 1518316276430466929,  3,  191084684, Package.Prototype)
      };

      data[250756] = new List<SteamManifest> {
        new SteamManifest(257510, 257511, 1813793007507355582,  4,   84907936, Package.Main),
        new SteamManifest(257510, 257515, 1726824934552610516, 29, 6604599929, Package.Main),
        new SteamManifest(257510, 257516, 1646028800945546522,  4,     531616, Package.Main),
        new SteamManifest(257510, 257519, 7529726526274038229,  1,    2230935, Package.Main),
        new SteamManifest(257510, 358470, 5457565769128401790,  4,  896460334, Package.Gehenna),
        new SteamManifest(257510, 322022, 6332807582508063459,  3,  191084684, Package.Prototype)
      };

      data[252786] = new List<SteamManifest> {
        new SteamManifest(257510, 257511, 4797710291018151156,  4,   84907936, Package.Main),
        new SteamManifest(257510, 257515, 3411862904639819627, 29, 6604599937, Package.Main),
        new SteamManifest(257510, 257516, 1646028800945546522,  4,     531616, Package.Main),
        new SteamManifest(257510, 257519, 7754615816270172495,  1,    2230935, Package.Main),
        new SteamManifest(257510, 358470, 1181179272206183222,  4,  896460247, Package.Gehenna),
        new SteamManifest(257510, 322022, 6635364740942276299,  3,  191084684, Package.Prototype)
      };

      data[258375] = new List<SteamManifest> {
        new SteamManifest(257510, 257511, 0291313986193809433,  4,   84569504, Package.Main),
        new SteamManifest(257510, 257515, 6044225467093048211, 29, 6667251114, Package.Main),
        new SteamManifest(257510, 257516, 1646028800945546522,  4,     531616, Package.Main),
        new SteamManifest(257510, 257519, 1808951156598981957,  1,    2250567, Package.Main),
        new SteamManifest(257510, 358470, 5851055062358757678,  4,  896451088, Package.Gehenna),
        new SteamManifest(257510, 322022, 9214799288230159850,  3,  191280304, Package.Prototype)
      };

      data[260924] = new List<SteamManifest> {
        new SteamManifest(257510, 257511, 0499590387152058814,  4,   86096800, Package.Main),
        new SteamManifest(257510, 257515, 8206832965751498451, 29, 6667399585, Package.Main),
        new SteamManifest(257510, 257516, 1646028800945546522,  4,     531616, Package.Main),
        new SteamManifest(257510, 257519, 3398982331085565137,  1,    2250607, Package.Main),
        new SteamManifest(257510, 358470, 4919986680879759016,  4,  902448838, Package.Gehenna),
        new SteamManifest(257510, 322022, 7503933396680891180,  3,  191280306, Package.Prototype)
      };

      data[264510] = new List<SteamManifest> {
        new SteamManifest(257510, 257511, 4661937614683834332,  4,   86476576, Package.Main),
        new SteamManifest(257510, 257515, 7995925049155399775, 29, 6677789906, Package.Main),
        new SteamManifest(257510, 257516, 0827084533033961837,  6,    8172400, Package.Main),
        new SteamManifest(257510, 257519, 2063011968231605882,  1,    2253792, Package.Main),
        new SteamManifest(257510, 358470, 8410067891696867813,  4,  902448650, Package.Gehenna),
        new SteamManifest(257510, 322022, 5194706371718824589,  3,  191280314, Package.Prototype)
      };

      data[267252] = new List<SteamManifest> {
        new SteamManifest(257510, 257511, 0914480456153849155,  4,   86718752, Package.Main),
        new SteamManifest(257510, 257515, 8897416572811879523, 29, 6704813319, Package.Main),
        new SteamManifest(257510, 257516, 0827084533033961837,  6,    8172400, Package.Main),
        new SteamManifest(257510, 257519, 3339621383432167258,  1,    2262052, Package.Main),
        new SteamManifest(257510, 358470, 7441510589944616318,  4,  902448620, Package.Gehenna),
        new SteamManifest(257510, 322022, 3417810446044054571,  3,  191280318, Package.Prototype)
      };

      data[269335] = new List<SteamManifest> {
        new SteamManifest(257510, 257511, 4361546767985939876,  4,   87218976, Package.Main),
        new SteamManifest(257510, 257515, 2778697671525586970, 29, 6704324599, Package.Main),
        new SteamManifest(257510, 257516, 0827084533033961837,  6,    8172400, Package.Main),
        new SteamManifest(257510, 257519, 3429893718454775303,  1,    2261892, Package.Main),
        new SteamManifest(257510, 358470, 2971770452830915701,  4,  902448648, Package.Gehenna),
        new SteamManifest(257510, 322022, 2804553596758319623,  3,  191280313, Package.Prototype)
      };

      data[277544] = new List<SteamManifest> {
        new SteamManifest(257510, 257511, 4438368920362581908,  4,   87489312, Package.Main),
        new SteamManifest(257510, 257515, 8527295741256308631, 29, 6701407239, Package.Main),
        new SteamManifest(257510, 257516, 0827084533033961837,  6,    8172400, Package.Main),
        new SteamManifest(257510, 257519, 8707002697175431137,  1,    2261012, Package.Main),
        new SteamManifest(257510, 358470, 5026831778825708639,  4,  902535678, Package.Gehenna),
        new SteamManifest(257510, 322022, 4959862740532303621,  3,  191280317, Package.Prototype)
      };

      data[284152] = new List<SteamManifest> {
        new SteamManifest(257510, 257511, 2310534095082994736,  4,   87675168, Package.Main),
        new SteamManifest(257510, 257515, 6023749242906446787, 29, 6701585582, Package.Main),
        new SteamManifest(257510, 257516, 0827084533033961837,  6,     8172400, Package.Main),
        new SteamManifest(257510, 257519, 3309514767764105231,  1,    2261052, Package.Main),
        new SteamManifest(257510, 358470, 6462721285409783950,  4,  902535584, Package.Gehenna),
        new SteamManifest(257510, 322022, 6922743269332565255,  3,  191280316, Package.Prototype)
      };

      data[291145] = new List<SteamManifest> {
        new SteamManifest(257510, 257511, 2517236650746590986,  4,   87096608, Package.Main),
        new SteamManifest(257510, 257515, 5170392634591662639, 29, 6726169973, Package.Main),
        new SteamManifest(257510, 257516, 0827084533033961837,  6,    8172400, Package.Main),
        new SteamManifest(257510, 257519, 5158478294883956711,  1,    2268552, Package.Main),
        new SteamManifest(257510, 358470, 3878145609496862471,  4,  902535704, Package.Gehenna),
        new SteamManifest(257510, 322022, 0330496884825252439,  3,  191280317, Package.Prototype)
      };

      data[293384] = new List<SteamManifest> {
        new SteamManifest(257510, 257511, 7465431023497269772,  4,   87130912, Package.Main),
        new SteamManifest(257510, 257515, 3921528168366102418, 29, 6701195630, Package.Main),
        new SteamManifest(257510, 257516, 0827084533033961837,  6,    8172400, Package.Main),
        new SteamManifest(257510, 257519, 2400271216751877709,  1,    2260932, Package.Main),
        new SteamManifest(257510, 358470, 3689687611838010639,  4,  902535631, Package.Gehenna),
        new SteamManifest(257510, 322022, 6864442709661062902,  3,  191280316, Package.Prototype)
      };

      data[300763] = new List<SteamManifest> {
        new SteamManifest(257510, 257511, 0236601650031060048,  4,   87206688, Package.Main),
        new SteamManifest(257510, 257515, 3706310912589354532, 29, 6701203978, Package.Main),
        new SteamManifest(257510, 257516, 0827084533033961837,  6,    8172400, Package.Main),
        new SteamManifest(257510, 257519, 8838046835968752341,  1,    2260932, Package.Main),
        new SteamManifest(257510, 358470, 2608271957197798008,  4,  902535566, Package.Gehenna),
        new SteamManifest(257510, 322022, 1944742430107480164,  3,  191280316, Package.Prototype)
      };

      data[301136] = new List<SteamManifest> {
        new SteamManifest(257510, 257511, 5351514903097441265,  4,   87213344, Package.Main),
        new SteamManifest(257510, 257515, 0518026948382674055, 29, 6701203091, Package.Main),
        new SteamManifest(257510, 257516, 0827084533033961837,  6,    8172400, Package.Main),
        new SteamManifest(257510, 257519, 3588926819998531108,  1,    2260932, Package.Main),
        new SteamManifest(257510, 358470, 1599787433380595759,  4,  902535677, Package.Gehenna),
        new SteamManifest(257510, 322022, 2866879691166241287,  3,  191280316, Package.Prototype)
      };

      data[326589] = new List<SteamManifest> {
        new SteamManifest(257510, 257511, 7222986970071829007,  4,   91624224, Package.Main),
        new SteamManifest(257510, 257515, 3164152563056127398, 29, 6770687163, Package.Main),
        new SteamManifest(257510, 257516, 0827084533033961837,  6,    8172400, Package.Main),
        new SteamManifest(257510, 257519, 7686943783455980924,  1,    2282132, Package.Main),
        new SteamManifest(257510, 358470, 5178136109328124898,  4,  902817535, Package.Gehenna),
        new SteamManifest(257510, 322022, 1999687630096794293,  3,  191280306, Package.Prototype)
      };

      data[424910] = new List<SteamManifest> {
        new SteamManifest(257510, 257511, 4445925190137047197,  2,   66084984, Package.Main),
        new SteamManifest(257510, 257515, 5833630477596020402, 49, 5634581190, Package.Main),
        new SteamManifest(257510, 257516, 5450066894376160295, 11,   33548240, Package.Main),
        new SteamManifest(257510, 358470, 0446988425271855249,  2,  777810959, Package.Gehenna),
        new SteamManifest(257510, 322022, 1251221992794837269,  2,  201570650, Package.Prototype)
      };

      data[426014] = new List<SteamManifest> {
        new SteamManifest(257510, 257511, 3586505834137878681,  2,   66084976, Package.Main),
        new SteamManifest(257510, 257515, 6301775319056368422, 50, 5663034799, Package.Main),
        new SteamManifest(257510, 257516, 8705936455967316907, 11,   33548240, Package.Main),
        new SteamManifest(257510, 358470, 3702226411907956172,  3,  786618977, Package.Gehenna),
        new SteamManifest(257510, 322022, 1538185595447097219,  2,  201570963, Package.Prototype)
      };

      data[429074] = new List<SteamManifest> {
        new SteamManifest(257510, 257511, 8159846362257674313,  2,   66084984, Package.Main),
        new SteamManifest(257510, 257515, 2713051319254128482, 50, 5663034511, Package.Main),
        new SteamManifest(257510, 257516, 3680540037408603213, 11,   33548240, Package.Main),
        new SteamManifest(257510, 358470, 6406142202705981356,  3,  786732859, Package.Gehenna),
        new SteamManifest(257510, 322022, 6956365166520441230,  2,  201571015, Package.Prototype)
      };

      data[440323] = new List<SteamManifest> {
        new SteamManifest(257510, 257511, 0799213806328220919,  2,   66117856, Package.Main),
        new SteamManifest(257510, 257515, 3279814669572335644, 52, 5667320739, Package.Main),
        new SteamManifest(257510, 257516, 7924825898116512954, 12,   34207184, Package.Main),
        new SteamManifest(257510, 322022, 8229910699963260352,  2,  201589569, Package.Prototype),
        new SteamManifest(257510, 358470, 0981576150363927305,  3,  883262060, Package.Gehenna)
      };

      System.Diagnostics.Debug.Assert(
        data.Keys.All(ver => allVersions.Contains(ver)) && data.Keys.Count == allVersions.Count,
        "Mismatch between versions in 'data' and 'allVersions'"
      );

      return data;
    }
  }
}