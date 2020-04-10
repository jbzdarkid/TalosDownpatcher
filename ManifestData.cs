using Microsoft.Win32;
using System.Collections.Generic;

namespace TalosDownpatcher {
  using ManifestDict = Dictionary<int, Dictionary<Package, List<SteamManifest>>>;

  public class SteamManifest {
    public readonly Package package;
    public readonly int appId;
    public readonly int depotId;
    public readonly long manifestId;
    public readonly int numFiles; // @Cleanup: Unused, but definitely should be (at least for validation).
    public readonly long size;
    public readonly string location;

    public SteamManifest(Package package, int appId, int depotId, long manifestId, int numFiles, long size) {
      this.package = package;
      this.appId = appId;
      this.depotId = depotId;
      this.manifestId = manifestId;
      this.numFiles = numFiles;
      this.size = size;
      this.location = $"{ManifestData.DepotLocation}/app_{appId}/depot_{depotId}";
    }
  }

  public static class ManifestData {
    public static readonly List<int> allVersions = new List<int> {
      440323, 429074, 426014, 424910, 326589, 301136, 300763, 293384,
      291145, 284152, 277544, 269335, 267252, 264510, 260924, 258375,
      252786, 250756, 249913, 249740, 248828, 248139, 246379, 244371,
      243520, 226087, 224995, 224531, 223249, 222477, 221394, 220996,
      220675, 220625, 220480
    };
    public static readonly string DepotLocation;
    private static readonly ManifestDict data;

    // Helper function so that data can be private. This also helps the runtime avoid accidentally copying data[version] as an intermediate.
    public static List<SteamManifest> Get(int version, Package package) {
      return data[version][package];
    }

    public static long GetDownloadSize(int version, Package package) {
      long totalDownloadSize = 0;
      foreach (var manifest in Get(version, package)) totalDownloadSize += manifest.size;
      return totalDownloadSize;
    }

    private static void AddManifest(int version, Package package, int appId, int depotId, long manifestId, int numFiles, long size) {
      data[version][package].Add(new SteamManifest(package, appId, depotId, manifestId, numFiles, size));
    }

    static ManifestData() {
      string steamInstall = (string)Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Valve\Steam", "SteamPath", "C:/Program Files (x86)/Steam");
      DepotLocation = $"{steamInstall}/steamapps/content";

      data = new ManifestDict();
      foreach (var version in allVersions) {
        data[version] = new Dictionary<Package, List<SteamManifest>>();
        data[version][Package.Main] = new List<SteamManifest>();
        data[version][Package.Gehenna] = new List<SteamManifest>();
        data[version][Package.Prototype] = new List<SteamManifest>();
        data[version][Package.Editor] = new List<SteamManifest>();
      }

      AddManifest(440323, Package.Main, 257510, 257511, 0799213806328220919, 2, 66117856);
      AddManifest(429074, Package.Main, 257510, 257511, 8159846362257674313, 2, 66084984);
      AddManifest(426014, Package.Main, 257510, 257511, 3586505834137878681, 2, 66084976);
      AddManifest(424910, Package.Main, 257510, 257511, 4445925190137047197, 2, 66084984);
      AddManifest(326589, Package.Main, 257510, 257511, 7222986970071829007, 4, 91624224);
      AddManifest(301136, Package.Main, 257510, 257511, 5351514903097441265, 4, 87213344);
      AddManifest(300763, Package.Main, 257510, 257511, 0236601650031060048, 4, 87206688);
      AddManifest(293384, Package.Main, 257510, 257511, 7465431023497269772, 4, 87130912);
      AddManifest(291145, Package.Main, 257510, 257511, 2517236650746590986, 4, 87096608);
      AddManifest(284152, Package.Main, 257510, 257511, 2310534095082994736, 4, 87675168);
      AddManifest(277544, Package.Main, 257510, 257511, 4438368920362581908, 4, 87489312);
      AddManifest(269335, Package.Main, 257510, 257511, 4361546767985939876, 4, 87218976);
      AddManifest(267252, Package.Main, 257510, 257511, 0914480456153849155, 4, 86718752);
      AddManifest(264510, Package.Main, 257510, 257511, 4661937614683834332, 4, 86476576);
      AddManifest(260924, Package.Main, 257510, 257511, 0499590387152058814, 4, 86096800);
      AddManifest(258375, Package.Main, 257510, 257511, 0291313986193809433, 4, 84569504);
      AddManifest(252786, Package.Main, 257510, 257511, 4797710291018151156, 4, 84907936);
      AddManifest(250756, Package.Main, 257510, 257511, 1813793007507355582, 4, 84907936);
      AddManifest(249913, Package.Main, 257510, 257511, 0104511117861601608, 2, 37954256);
      AddManifest(249740, Package.Main, 257510, 257511, 2372465056935606724, 2, 37954768);
      AddManifest(248828, Package.Main, 257510, 257511, 4259291835568024105, 2, 37933264);
      AddManifest(248139, Package.Main, 257510, 257511, 1500898004646373285, 2, 37932240);
      AddManifest(246379, Package.Main, 257510, 257511, 7720637873729107634, 2, 37919440);
      AddManifest(244371, Package.Main, 257510, 257511, 7689461949196252397, 2, 37835472);
      AddManifest(243520, Package.Main, 257510, 257511, 0486897345253495735, 2, 37835472);
      AddManifest(226087, Package.Main, 257510, 257511, 2501832898327740448, 2, 38003408);
      AddManifest(224995, Package.Main, 257510, 257511, 9130874505093356390, 2, 38000336);
      AddManifest(224531, Package.Main, 257510, 257511, 3207947001105705384, 2, 37991120);
      AddManifest(223249, Package.Main, 257510, 257511, 4657839721186080333, 2, 49014480);
      AddManifest(222477, Package.Main, 257510, 257511, 0918811395787437543, 2, 49005264);
      AddManifest(221394, Package.Main, 257510, 257511, 1222567553988779631, 2, 48947920);
      AddManifest(220996, Package.Main, 257510, 257511, 3475103749484918333, 2, 48912080);
      AddManifest(220675, Package.Main, 257510, 257511, 2018192542376932921, 2, 48900816);
      AddManifest(220625, Package.Main, 257510, 257511, 8087374737096894864, 2, 48901328);
      AddManifest(220480, Package.Main, 257510, 257511, 4122334240058475272, 2, 48908496);

      AddManifest(440323, Package.Main, 257510, 257515, 3279814669572335644, 52, 5667320739);
      AddManifest(429074, Package.Main, 257510, 257515, 2713051319254128482, 50, 5663034511);
      AddManifest(426014, Package.Main, 257510, 257515, 6301775319056368422, 50, 5663034799);
      AddManifest(424910, Package.Main, 257510, 257515, 5833630477596020402, 49, 5634581190);
      AddManifest(326589, Package.Main, 257510, 257515, 3164152563056127398, 29, 6770687163);
      AddManifest(301136, Package.Main, 257510, 257515, 0518026948382674055, 29, 6701203091);
      AddManifest(300763, Package.Main, 257510, 257515, 3706310912589354532, 29, 6701203978);
      AddManifest(293384, Package.Main, 257510, 257515, 3921528168366102418, 29, 6701195630);
      AddManifest(291145, Package.Main, 257510, 257515, 5170392634591662639, 29, 6726169973);
      AddManifest(284152, Package.Main, 257510, 257515, 6023749242906446787, 29, 6701585582);
      AddManifest(277544, Package.Main, 257510, 257515, 8527295741256308631, 29, 6701407239);
      AddManifest(269335, Package.Main, 257510, 257515, 2778697671525586970, 29, 6704324599);
      AddManifest(267252, Package.Main, 257510, 257515, 8897416572811879523, 29, 6704813319);
      AddManifest(264510, Package.Main, 257510, 257515, 7995925049155399775, 29, 6677789906);
      AddManifest(260924, Package.Main, 257510, 257515, 8206832965751498451, 29, 6667399585);
      AddManifest(258375, Package.Main, 257510, 257515, 6044225467093048211, 29, 6667251114);
      AddManifest(252786, Package.Main, 257510, 257515, 3411862904639819627, 29, 6604599937);
      AddManifest(250756, Package.Main, 257510, 257515, 1726824934552610516, 29, 6604599929);
      AddManifest(249913, Package.Main, 257510, 257515, 8145595254539830554, 29, 6590576890);
      AddManifest(249740, Package.Main, 257510, 257515, 7774619190361198089, 29, 6590576873);
      AddManifest(248828, Package.Main, 257510, 257515, 5103279241419561908, 30, 6589271132);
      AddManifest(248139, Package.Main, 257510, 257515, 8022908710052117236, 30, 6589267934);
      AddManifest(246379, Package.Main, 257510, 257515, 4896296030980495433, 29, 6546725617);
      AddManifest(244371, Package.Main, 257510, 257515, 8579671038386532826, 28, 6427365830);
      AddManifest(243520, Package.Main, 257510, 257515, 0762065295327824568, 29, 6870004878);
      AddManifest(226087, Package.Main, 257510, 257515, 3826319505876650203, 29, 6282258753);
      AddManifest(224995, Package.Main, 257510, 257515, 4840744566182207229, 28, 6281389510);
      AddManifest(224531, Package.Main, 257510, 257515, 5798939509479840711, 28, 6281382910);
      AddManifest(223249, Package.Main, 257510, 257515, 8304752583573271963, 27, 6094446741);
      AddManifest(222477, Package.Main, 257510, 257515, 6267514976170907571, 27, 6094446568);
      AddManifest(221394, Package.Main, 257510, 257515, 7204783264154690101, 27, 5842288583);
      AddManifest(220996, Package.Main, 257510, 257515, 0415067456888111848, 27, 5439377389);
      AddManifest(220675, Package.Main, 257510, 257515, 8986158524600611840, 32, 5217247413);
      AddManifest(220625, Package.Main, 257510, 257515, 3617089317964903627, 32, 5217247608);
      AddManifest(220480, Package.Main, 257510, 257515, 1348826399794024471, 32, 5055625401);

      AddManifest(440323, Package.Main, 257510, 257516, 7924825898116512954, 12, 34207184);
      AddManifest(429074, Package.Main, 257510, 257516, 3680540037408603213, 11, 33548240);
      AddManifest(426014, Package.Main, 257510, 257516, 8705936455967316907, 11, 33548240);
      AddManifest(424910, Package.Main, 257510, 257516, 5450066894376160295, 11, 33548240);
      AddManifest(326589, Package.Main, 257510, 257516, 0827084533033961837, 6, 8172400);
      AddManifest(301136, Package.Main, 257510, 257516, 0827084533033961837, 6, 8172400);
      AddManifest(300763, Package.Main, 257510, 257516, 0827084533033961837, 6, 8172400);
      AddManifest(293384, Package.Main, 257510, 257516, 0827084533033961837, 6, 8172400);
      AddManifest(291145, Package.Main, 257510, 257516, 0827084533033961837, 6, 8172400);
      AddManifest(284152, Package.Main, 257510, 257516, 0827084533033961837, 6, 8172400);
      AddManifest(277544, Package.Main, 257510, 257516, 0827084533033961837, 6, 8172400);
      AddManifest(269335, Package.Main, 257510, 257516, 0827084533033961837, 6, 8172400);
      AddManifest(267252, Package.Main, 257510, 257516, 0827084533033961837, 6, 8172400);
      AddManifest(264510, Package.Main, 257510, 257516, 0827084533033961837, 6, 8172400);
      AddManifest(260924, Package.Main, 257510, 257516, 1646028800945546522, 4, 531616);
      AddManifest(258375, Package.Main, 257510, 257516, 1646028800945546522, 4, 531616);
      AddManifest(252786, Package.Main, 257510, 257516, 1646028800945546522, 4, 531616);
      AddManifest(250756, Package.Main, 257510, 257516, 1646028800945546522, 4, 531616);
      AddManifest(249913, Package.Main, 257510, 257516, 7546122753718189131, 2, 250448);
      AddManifest(249740, Package.Main, 257510, 257516, 7546122753718189131, 2, 250448);
      AddManifest(248828, Package.Main, 257510, 257516, 0537534056471328674, 2, 208576);
      AddManifest(248139, Package.Main, 257510, 257516, 0537534056471328674, 2, 208576);
      AddManifest(246379, Package.Main, 257510, 257516, 0537534056471328674, 2, 208576);
      AddManifest(244371, Package.Main, 257510, 257516, 7901792711762181252, 7, 218389);
      AddManifest(243520, Package.Main, 257510, 257516, 7901792711762181252, 7, 218389);
      AddManifest(226087, Package.Main, 257510, 257516, 1237958166729860756, 6, 117245);
      AddManifest(224995, Package.Main, 257510, 257516, 1237958166729860756, 6, 117245);
      AddManifest(224531, Package.Main, 257510, 257516, 1237958166729860756, 6, 117245);
      AddManifest(223249, Package.Main, 257510, 257516, 1237958166729860756, 6, 117245);
      AddManifest(222477, Package.Main, 257510, 257516, 1237958166729860756, 6, 117245);
      AddManifest(221394, Package.Main, 257510, 257516, 1237958166729860756, 6, 117245);
      AddManifest(220996, Package.Main, 257510, 257516, 1237958166729860756, 6, 117245);
      AddManifest(220675, Package.Main, 257510, 257516, 1237958166729860756, 6, 117245);
      AddManifest(220625, Package.Main, 257510, 257516, 1237958166729860756, 6, 117245);
      AddManifest(220480, Package.Main, 257510, 257516, 1237958166729860756, 6, 117245);

      AddManifest(440323, Package.Main, 257510, 257519, 1170103677162582368, 0, 0);
      AddManifest(429074, Package.Main, 257510, 257519, 1170103677162582368, 0, 0);
      AddManifest(426014, Package.Main, 257510, 257519, 1170103677162582368, 0, 0);
      AddManifest(424910, Package.Main, 257510, 257519, 1170103677162582368, 0, 0);
      AddManifest(326589, Package.Main, 257510, 257519, 7686943783455980924, 1, 2282132);
      AddManifest(301136, Package.Main, 257510, 257519, 3588926819998531108, 1, 2260932);
      AddManifest(300763, Package.Main, 257510, 257519, 8838046835968752341, 1, 2260932);
      AddManifest(293384, Package.Main, 257510, 257519, 2400271216751877709, 1, 2260932);
      AddManifest(291145, Package.Main, 257510, 257519, 5158478294883956711, 1, 2268552);
      AddManifest(284152, Package.Main, 257510, 257519, 3309514767764105231, 1, 2261052);
      AddManifest(277544, Package.Main, 257510, 257519, 8707002697175431137, 1, 2261012);
      AddManifest(269335, Package.Main, 257510, 257519, 3429893718454775303, 1, 2261892);
      AddManifest(267252, Package.Main, 257510, 257519, 3339621383432167258, 1, 2262052);
      AddManifest(264510, Package.Main, 257510, 257519, 2063011968231605882, 1, 2253792);
      AddManifest(260924, Package.Main, 257510, 257519, 3398982331085565137, 1, 2250607);
      AddManifest(258375, Package.Main, 257510, 257519, 1808951156598981957, 1, 2250567);
      AddManifest(252786, Package.Main, 257510, 257519, 7754615816270172495, 1, 2230935);
      AddManifest(250756, Package.Main, 257510, 257519, 7529726526274038229, 1, 2230935);
      AddManifest(249913, Package.Main, 257510, 257519, 0264858112795519304, 1, 2226635);
      AddManifest(249740, Package.Main, 257510, 257519, 4648185825889483487, 1, 2226635);
      AddManifest(248828, Package.Main, 257510, 257519, 7130120369117951230, 1, 2226235);
      AddManifest(248139, Package.Main, 257510, 257519, 4793167457039384090, 1, 2226235);
      AddManifest(246379, Package.Main, 257510, 257519, 7776793304385903116, 1, 2213167);
      AddManifest(244371, Package.Main, 257510, 257519, 2675083093007158979, 1, 2175966);
      AddManifest(243520, Package.Main, 257510, 257519, 2020897382852338977, 1, 2312159);
      AddManifest(226087, Package.Main, 257510, 257519, 5081265059426016345, 1, 2117916);
      AddManifest(224995, Package.Main, 257510, 257519, 0660166059336687264, 1, 2117568);
      AddManifest(224531, Package.Main, 257510, 257519, 3565289474630779855, 1, 2117568);
      AddManifest(223249, Package.Main, 257510, 257519, 7835318215284690246, 1, 2059976);
      AddManifest(222477, Package.Main, 257510, 257519, 6671071736059004158, 1, 2059976);
      AddManifest(221394, Package.Main, 257510, 257519, 6019585236142413402, 1, 1981411);
      AddManifest(220996, Package.Main, 257510, 257519, 4906101680219337427, 1, 1858219);
      AddManifest(220675, Package.Main, 257510, 257519, 4554990112195176406, 1, 1789447);
      AddManifest(220625, Package.Main, 257510, 257519, 1272083368566319066, 1, 1789447);
      AddManifest(220480, Package.Main, 257510, 257519, 2322040020544521685, 1, 1739595);

      AddManifest(440323, Package.Gehenna, 257510, 358470, 0981576150363927305, 3, 883262060);
      AddManifest(429074, Package.Gehenna, 257510, 358470, 6406142202705981356, 3, 786732859);
      AddManifest(426014, Package.Gehenna, 257510, 358470, 3702226411907956172, 3, 786618977);
      AddManifest(424910, Package.Gehenna, 257510, 358470, 0446988425271855249, 2, 777810959);
      AddManifest(326589, Package.Gehenna, 257510, 358470, 5178136109328124898, 4, 902817535);
      AddManifest(301136, Package.Gehenna, 257510, 358470, 1599787433380595759, 4, 902535677);
      AddManifest(300763, Package.Gehenna, 257510, 358470, 2608271957197798008, 4, 902535566);
      AddManifest(293384, Package.Gehenna, 257510, 358470, 3689687611838010639, 4, 902535631);
      AddManifest(291145, Package.Gehenna, 257510, 358470, 3878145609496862471, 4, 902535704);
      AddManifest(284152, Package.Gehenna, 257510, 358470, 6462721285409783950, 4, 902535584);
      AddManifest(277544, Package.Gehenna, 257510, 358470, 5026831778825708639, 4, 902535678);
      AddManifest(269335, Package.Gehenna, 257510, 358470, 2971770452830915701, 4, 902448648);
      AddManifest(267252, Package.Gehenna, 257510, 358470, 7441510589944616318, 4, 902448620);
      AddManifest(264510, Package.Gehenna, 257510, 358470, 8410067891696867813, 4, 902448650);
      AddManifest(260924, Package.Gehenna, 257510, 358470, 4919986680879759016, 4, 902448838);
      AddManifest(258375, Package.Gehenna, 257510, 358470, 5851055062358757678, 4, 896451088);
      AddManifest(252786, Package.Gehenna, 257510, 358470, 1181179272206183222, 4, 896460247);
      AddManifest(250756, Package.Gehenna, 257510, 358470, 5457565769128401790, 4, 896460334);
      AddManifest(249913, Package.Gehenna, 257510, 358470, 2832266850408399266, 4, 896460324);
      AddManifest(249740, Package.Gehenna, 257510, 358470, 6996102492712095516, 4, 896460342);
      AddManifest(248828, Package.Gehenna, 257510, 358470, 1215334353744988752, 4, 896460268);
      AddManifest(248139, Package.Gehenna, 257510, 358470, 9140920440213320364, 4, 896460286);
      AddManifest(246379, Package.Gehenna, 257510, 358470, 7419870507570765883, 3, 866850779);
      AddManifest(244371, Package.Gehenna, 257510, 358470, 8176613815265754262, 3, 749886317);
      AddManifest(243520, Package.Gehenna, 257510, 358470, 3645040643832281528, 2, 704403859);
      AddManifest(226087, Package.Gehenna, 257510, 358470, 0, 0, 0);
      AddManifest(224995, Package.Gehenna, 257510, 358470, 0, 0, 0);
      AddManifest(224531, Package.Gehenna, 257510, 358470, 0, 0, 0);
      AddManifest(223249, Package.Gehenna, 257510, 358470, 0, 0, 0);
      AddManifest(222477, Package.Gehenna, 257510, 358470, 0, 0, 0);
      AddManifest(221394, Package.Gehenna, 257510, 358470, 0, 0, 0);
      AddManifest(220996, Package.Gehenna, 257510, 358470, 0, 0, 0);
      AddManifest(220675, Package.Gehenna, 257510, 358470, 0, 0, 0);
      AddManifest(220625, Package.Gehenna, 257510, 358470, 0, 0, 0);
      AddManifest(220480, Package.Gehenna, 257510, 358470, 0, 0, 0);

      AddManifest(440323, Package.Prototype, 257510, 322022, 8229910699963260352, 2, 201589569);
      AddManifest(429074, Package.Prototype, 257510, 322022, 6956365166520441230, 2, 201571015);
      AddManifest(426014, Package.Prototype, 257510, 322022, 1538185595447097219, 2, 201570963);
      AddManifest(424910, Package.Prototype, 257510, 322022, 1251221992794837269, 2, 201570650);
      AddManifest(326589, Package.Prototype, 257510, 322022, 1999687630096794293, 3, 191280306);
      AddManifest(301136, Package.Prototype, 257510, 322022, 2866879691166241287, 3, 191280316);
      AddManifest(300763, Package.Prototype, 257510, 322022, 1944742430107480164, 3, 191280316);
      AddManifest(293384, Package.Prototype, 257510, 322022, 6864442709661062902, 3, 191280316);
      AddManifest(291145, Package.Prototype, 257510, 322022, 0330496884825252439, 3, 191280317);
      AddManifest(284152, Package.Prototype, 257510, 322022, 6922743269332565255, 3, 191280316);
      AddManifest(277544, Package.Prototype, 257510, 322022, 4959862740532303621, 3, 191280317);
      AddManifest(269335, Package.Prototype, 257510, 322022, 2804553596758319623, 3, 191280313);
      AddManifest(267252, Package.Prototype, 257510, 322022, 3417810446044054571, 3, 191280318);
      AddManifest(264510, Package.Prototype, 257510, 322022, 5194706371718824589, 3, 191280314);
      AddManifest(260924, Package.Prototype, 257510, 322022, 7503933396680891180, 3, 191280306);
      AddManifest(258375, Package.Prototype, 257510, 322022, 9214799288230159850, 3, 191280304);
      AddManifest(252786, Package.Prototype, 257510, 322022, 6635364740942276299, 3, 191084684);
      AddManifest(250756, Package.Prototype, 257510, 322022, 6332807582508063459, 3, 191084684);
      AddManifest(249913, Package.Prototype, 257510, 322022, 1518316276430466929, 3, 191084684);
      AddManifest(249740, Package.Prototype, 257510, 322022, 5134711953944719494, 3, 191084684);
      AddManifest(248828, Package.Prototype, 257510, 322022, 6408748820177783708, 3, 191084683);
      AddManifest(248139, Package.Prototype, 257510, 322022, 5515868016021140366, 3, 191084683);
      AddManifest(246379, Package.Prototype, 257510, 322022, 4423801769448698176, 2, 191045699);
      AddManifest(244371, Package.Prototype, 257510, 322022, 0570457731767330233, 2, 191173592);
      AddManifest(243520, Package.Prototype, 257510, 322022, 1221230661228297401, 4, 291798721);

      AddManifest(440323, Package.Editor, 257560, 257561, 6074611688291943692, 6, 66744536);
      AddManifest(429074, Package.Editor, 257560, 257561, 0012535541839910782, 6, 66712688);
      AddManifest(426014, Package.Editor, 257560, 257561, 9032863148474862611, 6, 66712688);
      AddManifest(424910, Package.Editor, 257560, 257561, 6380867156067713873, 6, 66712696);
      AddManifest(326589, Package.Editor, 257560, 257561, 7403469436702737328, 10, 102242736);
      AddManifest(301136, Package.Editor, 257560, 257561, 1853831671363106406, 10, 100045232);
      AddManifest(300763, Package.Editor, 257560, 257561, 4419631978316278784, 10, 100042160);
      AddManifest(293384, Package.Editor, 257560, 257561, 3937516059174114109, 10, 100001200);
      AddManifest(291145, Package.Editor, 257560, 257561, 2774799663803499998, 10, 99983280);
      AddManifest(284152, Package.Editor, 257560, 257561, 1024129366064843273, 10, 100262832);
      AddManifest(277544, Package.Editor, 257560, 257561, 0232702167893949832, 10, 100175280);
      AddManifest(269335, Package.Editor, 257560, 257561, 4443819059974101813, 10, 100025776);
      AddManifest(267252, Package.Editor, 257560, 257561, 7478870879515759087, 10, 99771312);
      AddManifest(264510, Package.Editor, 257560, 257561, 6410945621576250013, 10, 99646896);
      AddManifest(260924, Package.Editor, 257560, 257561, 7235432741885730082, 10, 99372656);
      AddManifest(258375, Package.Editor, 257560, 257561, 6903110476929197927, 10, 97231472);
      AddManifest(252786, Package.Editor, 257560, 257561, 1690661485191386338, 8, 97143200);
      AddManifest(250756, Package.Editor, 257560, 257561, 5512540270319659673, 8, 97143200);
      AddManifest(249913, Package.Editor, 257560, 257561, 4867462938257249448, 4, 43313872);
      AddManifest(249740, Package.Editor, 257560, 257561, 0584477399320707358, 4, 43313360);
      AddManifest(248828, Package.Editor, 257560, 257561, 1427254775991481127, 4, 43300048);
      AddManifest(248139, Package.Editor, 257560, 257561, 2269154412409765367, 4, 43299536);
      AddManifest(246379, Package.Editor, 257560, 257561, 3201806512441871491, 4, 43573968);
      AddManifest(244371, Package.Editor, 257560, 257561, 5615026782384047935, 4, 43507920);
      AddManifest(243520, Package.Editor, 257560, 257561, 7629198744079585242, 4, 43507408);

      AddManifest(440323, Package.Editor, 257560, 257565, 3349372665361510613, 9, 818204885);
      AddManifest(429074, Package.Editor, 257560, 257565, 0561188624384138587, 9, 818083072);
      AddManifest(426014, Package.Editor, 257560, 257565, 2823013440787395799, 9, 818083072);
      AddManifest(424910, Package.Editor, 257560, 257565, 4575988063340670419, 9, 818083072);
      AddManifest(326589, Package.Editor, 257560, 257565, 0618742413915366044, 8, 640802027);
      AddManifest(301136, Package.Editor, 257560, 257565, 1208275055108540846, 8, 640831224);
      AddManifest(300763, Package.Editor, 257560, 257565, 6912166628361125702, 8, 640831224);
      AddManifest(293384, Package.Editor, 257560, 257565, 1715089954619827023, 8, 640831224);
      AddManifest(291145, Package.Editor, 257560, 257565, 8615466065357805889, 8, 640831224);
      AddManifest(284152, Package.Editor, 257560, 257565, 5053070112053004195, 8, 640831224);
      AddManifest(277544, Package.Editor, 257560, 257565, 1482229888993923370, 8, 640831224);
      AddManifest(269335, Package.Editor, 257560, 257565, 7806361588893739511, 8, 640831224);
      AddManifest(267252, Package.Editor, 257560, 257565, 2998209476115358794, 8, 640831224);
      AddManifest(264510, Package.Editor, 257560, 257565, 7725027497915974984, 8, 640831224);
      AddManifest(260924, Package.Editor, 257560, 257565, 5184604821950164019, 8, 640832314);
      AddManifest(258375, Package.Editor, 257560, 257565, 1720860299775178313, 8, 640832190);
      AddManifest(252786, Package.Editor, 257560, 257565, 4362986188065581554, 8, 640800676);
      AddManifest(250756, Package.Editor, 257560, 257565, 7444645619316095681, 8, 640800676);
      AddManifest(249913, Package.Editor, 257560, 257565, 8408498828499049481, 8, 640731382);
      AddManifest(249740, Package.Editor, 257560, 257565, 6370494515660992509, 8, 640731382);
      AddManifest(248828, Package.Editor, 257560, 257565, 8023134853166241418, 8, 640731145);
      AddManifest(248139, Package.Editor, 257560, 257565, 5416623746614877934, 8, 640731145);
      AddManifest(246379, Package.Editor, 257560, 257565, 2088321232340796377, 8, 640731181);
      AddManifest(244371, Package.Editor, 257560, 257565, 2037227228655442843, 8, 640099397);
      AddManifest(243520, Package.Editor, 257560, 257565, 9162957285291849883, 8, 640099332);
    }
  }
}