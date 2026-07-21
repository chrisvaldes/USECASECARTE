using Microsoft.AspNetCore.Components.Forms;

namespace Use_Case_Carte.Models;

public class InputModel
{
    public string TypeMag { get; set; }

    public DateTime StartPeriod { get; set; } = DateTime.Today;

    public DateTime EndPeriod { get; set; } = DateTime.Today;


    public IBrowserFile Apprint { get; set; }

    public IBrowserFile OpenAccount { get; set; }

    public IBrowserFile ActiveAccount { get; set; }

    public IBrowserFile ActivePackage { get; set; }

    public IBrowserFile CtxAccount { get; set; }

    public IBrowserFile DateLastSouPackEchu { get; set; }

    public IBrowserFile AccountHisDebiteByRedevCard { get; set; }
}