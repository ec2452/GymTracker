namespace Sustavzapracenjenapretkauteretani.Web.Models;

public class HomeDashboardViewModel
{
    // Counts
    public int VjezbeCount { get; set; }
    public int PlanoviCount { get; set; }
    public int KorisniciCount { get; set; }
    public int TreninziCount { get; set; }
    public int CiljeviCount { get; set; }
    public int MjerenjaCount { get; set; }

    // Mjerenja / težina
    public double? ZadnjaTezina { get; set; }
    public double? TezinaDelta30Dana { get; set; }
    public DateTime? ZadnjeMjerenjeDate { get; set; }
    public double? CiljanaTezina { get; set; }
    public string? SparklinePoints { get; set; }

    // Treninzi stats
    public int TreninziOvajTjedan { get; set; }
    public double? ProsjecnaOcjena { get; set; }
    public int ProsjecnoTrajanje { get; set; }

    // Ciljevi stats
    public int CiljeviPostignutiCount { get; set; }

    // Korisnici stats
    public int KorisniciOvajMjesecCount { get; set; }

    // Computed helpers
    public double? DoCirclja => CiljanaTezina.HasValue && ZadnjaTezina.HasValue
        ? CiljanaTezina.Value - ZadnjaTezina.Value
        : null;
}
