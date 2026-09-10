namespace StockControl.Dominio;

public static class ServicioInformes
{
    public static IEnumerable<InformeVenta> Filtrar(
        IEnumerable<InformeVenta> ventas,
        int? nroCaja,
        int? idCierre)
    {
        var consulta = ventas;
        if (idCierre is > 0)
            consulta = consulta.Where(v => v.IdCierre == idCierre);
        if (nroCaja is > 0)
            consulta = consulta.Where(v => v.NroCaja == nroCaja);
        return consulta;
    }
}
