﻿using System;
using System.Collections.Generic;

namespace TalosDownpatcher {
  public class Datum {
    public long manifest;
    public int numFiles;
    public long size;
    public long hash;
    public Datum(long manifest, int numFiles, long size, long hash) {
      this.manifest = manifest;
      this.numFiles = numFiles;
      this.hash = hash;
      this.size = size;
    }
  }
  public class Manifests {
    public static List<int> versions = new List<int> {
        429074, 426014, 424910, 326589, 301136, 300763, 293384,
        291145, 284152, 277544, 269335, 267252, 264510, 260924, 258375, 252786,
        250756, 249913, 249740, 248828, 248139, 246379, 244371, 243520, 226087,
        224995, 224531, 223249, 222477, 221394, 220996, 220675, 220625, 220480
    };

    // [Version][Depot] = {manifest, numFiles, numBytes, hash}
    public Dictionary<int, Dictionary<int, Datum>> data;

    public Manifests() {
      data = new Dictionary<int, Dictionary<int, Datum>>();
      foreach (var version in versions) {
        data[version] = new Dictionary<int, Datum>();
      }

      data[429074][257511] = new Datum(8159846362257674313, 5, 66084984, 123123);
      data[426014][257511] = new Datum(3586505834137878681, 5, 1000, 123123);
      data[424910][257511] = new Datum(4445925190137047197, 5, 1000, 123123);
      data[326589][257511] = new Datum(7222986970071829007, 5, 1000, 123123);
      data[301136][257511] = new Datum(5351514903097441265, 5, 1000, 123123);
      data[300763][257511] = new Datum(0236601650031060048, 5, 1000, 123123);
      data[293384][257511] = new Datum(7465431023497269772, 5, 1000, 123123);
      data[291145][257511] = new Datum(2517236650746590986, 5, 1000, 123123);
      data[284152][257511] = new Datum(2310534095082994736, 5, 1000, 123123);
      data[277544][257511] = new Datum(4438368920362581908, 5, 1000, 123123);
      data[269335][257511] = new Datum(4361546767985939876, 5, 1000, 123123);
      data[267252][257511] = new Datum(0914480456153849155, 5, 1000, 123123);
      data[264510][257511] = new Datum(4661937614683834332, 5, 1000, 123123);
      data[260924][257511] = new Datum(0499590387152058814, 5, 1000, 123123);
      data[258375][257511] = new Datum(0291313986193809433, 5, 1000, 123123);
      data[252786][257511] = new Datum(4797710291018151156, 5, 1000, 123123);
      data[250756][257511] = new Datum(1813793007507355582, 5, 1000, 123123);
      data[249913][257511] = new Datum(0104511117861601608, 5, 1000, 123123);
      data[249740][257511] = new Datum(2372465056935606724, 5, 1000, 123123);
      data[248828][257511] = new Datum(4259291835568024105, 5, 1000, 123123);
      data[248139][257511] = new Datum(1500898004646373285, 5, 1000, 123123);
      data[246379][257511] = new Datum(7720637873729107634, 5, 1000, 123123);
      data[244371][257511] = new Datum(7689461949196252397, 5, 1000, 123123);
      data[243520][257511] = new Datum(0486897345253495735, 5, 1000, 123123);
      data[226087][257511] = new Datum(2501832898327740448, 5, 1000, 123123);
      data[224995][257511] = new Datum(9130874505093356390, 5, 1000, 123123);
      data[224531][257511] = new Datum(3207947001105705384, 5, 1000, 123123);
      data[223249][257511] = new Datum(4657839721186080333, 5, 1000, 123123);
      data[222477][257511] = new Datum(0918811395787437543, 5, 1000, 123123);
      data[221394][257511] = new Datum(1222567553988779631, 5, 1000, 123123);
      data[220996][257511] = new Datum(3475103749484918333, 5, 1000, 123123);
      data[220675][257511] = new Datum(2018192542376932921, 5, 1000, 123123);
      data[220625][257511] = new Datum(8087374737096894864, 5, 1000, 123123);
      data[220480][257511] = new Datum(4122334240058475272, 5, 1000, 123123);

      data[429074][257515] = new Datum(2713051319254128482, 5, 60748820000000, 123123);
      data[426014][257515] = new Datum(6301775319056368422, 5, 1000, 123123);
      data[424910][257515] = new Datum(5833630477596020402, 5, 1000, 123123);
      data[326589][257515] = new Datum(3164152563056127398, 5, 1000, 123123);
      data[301136][257515] = new Datum(0518026948382674055, 5, 1000, 123123);
      data[300763][257515] = new Datum(3706310912589354532, 5, 1000, 123123);
      data[293384][257515] = new Datum(3921528168366102418, 5, 1000, 123123);
      data[291145][257515] = new Datum(5170392634591662639, 5, 1000, 123123);
      data[284152][257515] = new Datum(6023749242906446787, 5, 1000, 123123);
      data[277544][257515] = new Datum(8527295741256308631, 5, 1000, 123123);
      data[269335][257515] = new Datum(2778697671525586970, 5, 1000, 123123);
      data[267252][257515] = new Datum(8897416572811879523, 5, 1000, 123123);
      data[264510][257515] = new Datum(7995925049155399775, 5, 1000, 123123);
      data[260924][257515] = new Datum(8206832965751498451, 5, 1000, 123123);
      data[258375][257515] = new Datum(6044225467093048211, 5, 1000, 123123);
      data[252786][257515] = new Datum(3411862904639819627, 5, 1000, 123123);
      data[250756][257515] = new Datum(1726824934552610516, 5, 1000, 123123);
      data[249913][257515] = new Datum(8145595254539830554, 5, 1000, 123123);
      data[249740][257515] = new Datum(7774619190361198089, 5, 1000, 123123);
      data[248828][257515] = new Datum(5103279241419561908, 5, 1000, 123123);
      data[248139][257515] = new Datum(8022908710052117236, 5, 1000, 123123);
      data[246379][257515] = new Datum(4896296030980495433, 5, 1000, 123123);
      data[244371][257515] = new Datum(8579671038386532826, 5, 1000, 123123);
      data[243520][257515] = new Datum(0762065295327824568, 5, 1000, 123123);
      data[226087][257515] = new Datum(3826319505876650203, 5, 1000, 123123);
      data[224995][257515] = new Datum(4840744566182207229, 5, 1000, 123123);
      data[224531][257515] = new Datum(5798939509479840711, 5, 1000, 123123);
      data[223249][257515] = new Datum(8304752583573271963, 5, 1000, 123123);
      data[222477][257515] = new Datum(6267514976170907571, 5, 1000, 123123);
      data[221394][257515] = new Datum(7204783264154690101, 5, 1000, 123123);
      data[220996][257515] = new Datum(0415067456888111848, 5, 1000, 123123);
      data[220675][257515] = new Datum(8986158524600611840, 5, 1000, 123123);
      data[220625][257515] = new Datum(3617089317964903627, 5, 1000, 123123);
      data[220480][257515] = new Datum(1348826399794024471, 5, 1000, 123123);

      data[429074][257516] = new Datum(3680540037408603213, 5, 33548240, 123123);
      data[426014][257516] = new Datum(8705936455967316907, 5, 1000, 123123);
      data[424910][257516] = new Datum(5450066894376160295, 5, 1000, 123123);
      data[326589][257516] = new Datum(0827084533033961837, 5, 1000, 123123);
      data[301136][257516] = new Datum(0827084533033961837, 5, 1000, 123123);
      data[300763][257516] = new Datum(0827084533033961837, 5, 1000, 123123);
      data[293384][257516] = new Datum(0827084533033961837, 5, 1000, 123123);
      data[291145][257516] = new Datum(0827084533033961837, 5, 1000, 123123);
      data[284152][257516] = new Datum(0827084533033961837, 5, 1000, 123123);
      data[277544][257516] = new Datum(0827084533033961837, 5, 1000, 123123);
      data[269335][257516] = new Datum(0827084533033961837, 5, 1000, 123123);
      data[267252][257516] = new Datum(0827084533033961837, 5, 1000, 123123);
      data[264510][257516] = new Datum(0827084533033961837, 5, 1000, 123123);
      data[260924][257516] = new Datum(1646028800945546522, 5, 1000, 123123);
      data[258375][257516] = new Datum(1646028800945546522, 5, 1000, 123123);
      data[252786][257516] = new Datum(1646028800945546522, 5, 1000, 123123);
      data[250756][257516] = new Datum(1646028800945546522, 5, 1000, 123123);
      data[249913][257516] = new Datum(7546122753718189131, 5, 1000, 123123);
      data[249740][257516] = new Datum(7546122753718189131, 5, 1000, 123123);
      data[248828][257516] = new Datum(0537534056471328674, 5, 1000, 123123);
      data[248139][257516] = new Datum(0537534056471328674, 5, 1000, 123123);
      data[246379][257516] = new Datum(0537534056471328674, 5, 1000, 123123);
      data[244371][257516] = new Datum(7901792711762181252, 5, 1000, 123123);
      data[243520][257516] = new Datum(7901792711762181252, 5, 1000, 123123);
      data[226087][257516] = new Datum(1237958166729860756, 5, 1000, 123123);
      data[224995][257516] = new Datum(1237958166729860756, 5, 1000, 123123);
      data[224531][257516] = new Datum(1237958166729860756, 5, 1000, 123123);
      data[223249][257516] = new Datum(1237958166729860756, 5, 1000, 123123);
      data[222477][257516] = new Datum(1237958166729860756, 5, 1000, 123123);
      data[221394][257516] = new Datum(1237958166729860756, 5, 1000, 123123);
      data[220996][257516] = new Datum(1237958166729860756, 5, 1000, 123123);
      data[220675][257516] = new Datum(1237958166729860756, 5, 1000, 123123);
      data[220625][257516] = new Datum(1237958166729860756, 5, 1000, 123123);
      data[220480][257516] = new Datum(1237958166729860756, 5, 1000, 123123);

      data[429074][257519] = new Datum(1170103677162582368, 5, 100000000, 123123);
      data[426014][257519] = new Datum(1170103677162582368, 5, 1000, 123123);
      data[424910][257519] = new Datum(1170103677162582368, 5, 1000, 123123);
      data[326589][257519] = new Datum(7686943783455980924, 5, 1000, 123123);
      data[301136][257519] = new Datum(3588926819998531108, 5, 1000, 123123);
      data[300763][257519] = new Datum(8838046835968752341, 5, 1000, 123123);
      data[293384][257519] = new Datum(2400271216751877709, 5, 1000, 123123);
      data[291145][257519] = new Datum(5158478294883956711, 5, 1000, 123123);
      data[284152][257519] = new Datum(3309514767764105231, 5, 1000, 123123);
      data[277544][257519] = new Datum(8707002697175431137, 5, 1000, 123123);
      data[269335][257519] = new Datum(3429893718454775303, 5, 1000, 123123);
      data[267252][257519] = new Datum(3339621383432167258, 5, 1000, 123123);
      data[264510][257519] = new Datum(2063011968231605882, 5, 1000, 123123);
      data[260924][257519] = new Datum(3398982331085565137, 5, 1000, 123123);
      data[258375][257519] = new Datum(1808951156598981957, 5, 1000, 123123);
      data[252786][257519] = new Datum(7754615816270172495, 5, 1000, 123123);
      data[250756][257519] = new Datum(7529726526274038229, 5, 1000, 123123);
      data[249913][257519] = new Datum(0264858112795519304, 5, 1000, 123123);
      data[249740][257519] = new Datum(4648185825889483487, 5, 1000, 123123);
      data[248828][257519] = new Datum(7130120369117951230, 5, 1000, 123123);
      data[248139][257519] = new Datum(4793167457039384090, 5, 1000, 123123);
      data[246379][257519] = new Datum(7776793304385903116, 5, 1000, 123123);
      data[244371][257519] = new Datum(2675083093007158979, 5, 1000, 123123);
      data[243520][257519] = new Datum(2020897382852338977, 5, 1000, 123123);
      data[226087][257519] = new Datum(5081265059426016345, 5, 1000, 123123);
      data[224995][257519] = new Datum(0660166059336687264, 5, 1000, 123123);
      data[224531][257519] = new Datum(3565289474630779855, 5, 1000, 123123);
      data[223249][257519] = new Datum(7835318215284690246, 5, 1000, 123123);
      data[222477][257519] = new Datum(6671071736059004158, 5, 1000, 123123);
      data[221394][257519] = new Datum(6019585236142413402, 5, 1000, 123123);
      data[220996][257519] = new Datum(4906101680219337427, 5, 1000, 123123);
      data[220675][257519] = new Datum(4554990112195176406, 5, 1000, 123123);
      data[220625][257519] = new Datum(1272083368566319066, 5, 1000, 123123);
      data[220480][257519] = new Datum(2322040020544521685, 5, 1000, 123123);
    }
  }
}