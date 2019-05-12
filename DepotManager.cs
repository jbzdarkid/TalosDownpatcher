using System;
using System.Collections.Generic;
using System.IO;

namespace TalosDownpatcher {
  public class DepotManager {
    private static readonly Dictionary<int, List<long>> mainfestsForVersion = new Dictionary<int, List<long>> {
      {429074, new List<long>{8159846362257674313, 2713051319254128482, 3680540037408603213, 1170103677162582368}},
      {426014, new List<long>{3586505834137878681, 6301775319056368422, 8705936455967316907, 1170103677162582368}},
      {424910, new List<long>{4445925190137047197, 5833630477596020402, 5450066894376160295, 1170103677162582368}},
      {326589, new List<long>{7222986970071829007, 3164152563056127398,  827084533033961837, 7686943783455980924}},
      {301136, new List<long>{5351514903097441265,  518026948382674055,  827084533033961837, 3588926819998531108}},
      {300763, new List<long>{ 236601650031060048, 3706310912589354532,  827084533033961837, 8838046835968752341}},
      {293384, new List<long>{7465431023497269772, 3921528168366102418,  827084533033961837, 2400271216751877709}},
      {291145, new List<long>{2517236650746590986, 5170392634591662639,  827084533033961837, 5158478294883956711}},
      {284152, new List<long>{2310534095082994736, 6023749242906446787,  827084533033961837, 3309514767764105231}},
      {277544, new List<long>{4438368920362581908, 8527295741256308631,  827084533033961837, 8707002697175431137}},
      {269335, new List<long>{4361546767985939876, 2778697671525586970,  827084533033961837, 3429893718454775303}},
      {267252, new List<long>{ 914480456153849155, 8897416572811879523,  827084533033961837, 3339621383432167258}},
      {264510, new List<long>{4661937614683834332, 7995925049155399775,  827084533033961837, 2063011968231605882}},
      {260924, new List<long>{ 499590387152058814, 8206832965751498451, 1646028800945546522, 3398982331085565137}},
      {258375, new List<long>{ 291313986193809433, 6044225467093048211, 1646028800945546522, 1808951156598981957}},
      {252786, new List<long>{4797710291018151156, 3411862904639819627, 1646028800945546522, 7754615816270172495}},
      {250756, new List<long>{1813793007507355582, 1726824934552610516, 1646028800945546522, 7529726526274038229}},
      {249913, new List<long>{ 104511117861601608, 8145595254539830554, 7546122753718189131,  264858112795519304}},
      {249740, new List<long>{2372465056935606724, 7774619190361198089, 7546122753718189131, 4648185825889483487}},
      {248828, new List<long>{4259291835568024105, 5103279241419561908,  537534056471328674, 7130120369117951230}},
      {248139, new List<long>{1500898004646373285, 8022908710052117236,  537534056471328674, 4793167457039384090}},
      {246379, new List<long>{7720637873729107634, 4896296030980495433,  537534056471328674, 7776793304385903116}},
      {244371, new List<long>{7689461949196252397, 8579671038386532826, 7901792711762181252, 2675083093007158979}},
      {243520, new List<long>{ 486897345253495735,  762065295327824568, 7901792711762181252, 2020897382852338977}},
      {226087, new List<long>{2501832898327740448, 3826319505876650203, 1237958166729860756, 5081265059426016345}},
      {224995, new List<long>{9130874505093356390, 4840744566182207229, 1237958166729860756,  660166059336687264}},
      {224531, new List<long>{3207947001105705384, 5798939509479840711, 1237958166729860756, 3565289474630779855}},
      {223249, new List<long>{4657839721186080333, 8304752583573271963, 1237958166729860756, 7835318215284690246}},
      {222477, new List<long>{ 918811395787437543, 6267514976170907571, 1237958166729860756, 6671071736059004158}},
      {221394, new List<long>{1222567553988779631, 7204783264154690101, 1237958166729860756, 6019585236142413402}},
      {220996, new List<long>{3475103749484918333,  415067456888111848, 1237958166729860756, 4906101680219337427}},
      {220675, new List<long>{2018192542376932921, 8986158524600611840, 1237958166729860756, 4554990112195176406}},
      {220625, new List<long>{8087374737096894864, 3617089317964903627, 1237958166729860756, 1272083368566319066}},
      {220480, new List<long>{4122334240058475272, 1348826399794024471, 1237958166729860756, 2322040020544521685}}
    };

    private static readonly string steamapps = "C:/Program Files (x86)/Steam/steamapps"; // TODO: discover this, somehow
    private static readonly string activeVersionLocation = $"{steamapps}/common/The Talos Principle";
    private static readonly string oldVersionLocation = $"{steamapps}/common/The Talos Principle Old Versions";
    private static readonly string downloadedManifestsLocation = $"{oldVersionLocation}/downloadedManfiests.txt";
    private static readonly string depotLocation = $"{steamapps}/content/app_257510";

    private static readonly object downloadLock = new object();
    private List<long> downloadedManifests;
    private static readonly object versionLock = new object();
    private int activeVersion = 0;

    public DepotManager() {
      this.downloadedManifests = new List<long>();
      if (File.Exists(downloadedManifestsLocation)) {
        foreach (var line in File.ReadAllLines(downloadedManifestsLocation)) {
          this.downloadedManifests.Add(long.Parse(line));
        }
      }
    }

    private void SaveDownloadedManifests() {
      using (TextWriter tw = new StreamWriter(downloadedManifestsLocation)) {
        foreach (long manifest in this.downloadedManifests) {
            tw.WriteLine(manifest);
        }
      }
    }

    public int TrySetActiveVersion(int version) {
      lock (versionLock) {
        if (activeVersion == 0) {
          activeVersion = version;
          CopyAndReplace($"{oldVersionLocation}/{version}", activeVersionLocation);
        }
        return activeVersion;
      }
    }

    public void DownloadDepotsForVersion(int version) {
      List<long> manifests = mainfestsForVersion[version];
      lock (downloadLock) {
        DownloadDepot(version, 257511, manifests[0]);
        DownloadDepot(version, 257515, manifests[1]);
        DownloadDepot(version, 257516, manifests[2]);
        DownloadDepot(version, 257519, manifests[3]);
      }
    }

    private void DownloadDepot(int version, int depot, long manifest) {
      if (this.downloadedManifests.Contains(manifest)) return;

      SteamCommand.DownloadDepot(depot, manifest);
      MoveAndMerge($"{depotLocation}/depot_{depot}", $"{oldVersionLocation}/{version}");
      this.downloadedManifests.Add(manifest);
      SaveDownloadedManifests();
    }

    private static void MoveAndMerge(string sourceFolder, string destFolder) {
      Console.WriteLine($"Merging {sourceFolder} into {destFolder}");

      var src = new DirectoryInfo(sourceFolder);
      if (!src.Exists) return;
      var dst = new DirectoryInfo(destFolder);
      if (!dst.Exists) Directory.CreateDirectory(destFolder);

      foreach (var file in src.GetFiles()) {
        file.MoveTo($"{dst}/{file.Name}");
      }
      foreach (var dir in src.GetDirectories()) {
        MoveAndMerge($"{sourceFolder}/{dir}", $"{destFolder}/{dir}");
      }
    }

    private static void CopyAndReplace(string sourceFolder, string destFolder) {
      Console.WriteLine($"Deleting folder {destFolder} and overwriting with {sourceFolder}");
      Directory.Delete(destFolder);
      MoveAndMerge(sourceFolder, destFolder);
    }
  }
}
