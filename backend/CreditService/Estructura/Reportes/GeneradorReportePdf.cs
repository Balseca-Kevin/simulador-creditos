using System.Globalization;
using CreditService.Aplicacion.Contratos;
using CreditService.Aplicacion.Dtos;
using CreditService.Dominio;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CreditService.Estructura.Reportes;

/// <summary>
/// Maqueta el reporte de una simulación en PDF.
///
/// No calcula nada: recibe la simulación ya resuelta por el motor y solo la
/// presenta, de modo que el papel no puede decir una cifra distinta a la
/// pantalla.
/// </summary>
public class GeneradorReportePdf : IGeneradorReportePdf
{
    private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-EC");

    private const string Azul = "#2F5BB7";
    private const string AzulOscuro = "#28418A";
    private const string AzulClaro = "#EEF3FC";
    private const string Gris = "#64748B";
    private const string GrisClaro = "#F1F5F9";
    private const string Blanco = "#FFFFFF";

    public byte[] Generar(SimulacionResponse s, string? solicitante)
    {
        return Document.Create(documento =>
        {
            Portada(documento, s, solicitante);
            TablaMetodo(documento, s, s.Francesa, "Método Francés",
                "Cuota fija durante todo el plazo. Al inicio se paga más interés y, con el tiempo, más capital.");
            TablaMetodo(documento, s, s.Alemana, "Método Alemán",
                "Se amortiza la misma cantidad de capital en cada cuota, así que la cuota total decrece.");
        }).GeneratePdf();
    }

    /// <summary>Primera página: datos del crédito, resumen y comparativo de los dos métodos.</summary>
    private static void Portada(IDocumentContainer documento, SimulacionResponse s, string? solicitante)
    {
        documento.Page(pagina =>
        {
            Formato(pagina, s);

            pagina.Content().PaddingVertical(18).Column(col =>
            {
                col.Spacing(16);

                col.Item().Text("Reporte de simulación de crédito")
                    .FontSize(20).Bold().FontColor(AzulOscuro);

                if (!string.IsNullOrWhiteSpace(solicitante))
                {
                    col.Item().Text($"Solicitante: {solicitante}").FontSize(10).FontColor(Gris);
                }

                // --- Datos del crédito ---
                col.Item().Element(c => Seccion(c, "Datos del crédito"));
                col.Item().Table(t =>
                {
                    t.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); });

                    Dato(t, "Tipo de crédito", s.TipoCredito.Nombre);
                    Dato(t, "Categoría", s.TipoCredito.Categoria);
                    Dato(t, "Monto solicitado", Moneda(s.Monto));
                    Dato(t, "Plazo", $"{s.PlazoMeses} meses");
                    Dato(t, "Frecuencia de pago", s.FrecuenciaPago.Etiqueta());
                    Dato(t, "Número de cuotas", s.NumeroCuotas.ToString(Cultura));
                    Dato(t, "Tasa de interés anual", Porcentaje(s.TasaAnualAplicada));
                    Dato(t, "Tasa del período", Porcentaje(s.TasaPeriodicaAplicada * 100m, 6));
                    Dato(t, "Seguro de desgravamen",
                        s.IncluyeSeguroDesgravamen
                            ? $"Incluido, {Porcentaje(s.TipoCredito.TasaSeguroDesgravamenMensual, 4)} mensual sobre el saldo"
                            : "No incluido");
                    Dato(t, "Fecha de la simulación", s.FechaSimulacion.ToLocalTime().ToString("dd/MM/yyyy HH:mm", Cultura));
                });

                // --- Comparativo ---
                col.Item().Element(c => Seccion(c, "Comparación de los dos métodos"));
                col.Item().Table(t =>
                {
                    t.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(2.2f);
                        c.RelativeColumn();
                        c.RelativeColumn();
                    });

                    t.Header(h =>
                    {
                        h.Cell().Element(Encabezado).AlignLeft().Text("Concepto");
                        h.Cell().Element(Encabezado).AlignRight().Text("Francés");
                        h.Cell().Element(Encabezado).AlignRight().Text("Alemán");
                    });

                    // En las cuotas no se resalta nada: una cuota más baja no es
                    // necesariamente mejor, porque suele significar pagar más
                    // intereses a lo largo del plazo.
                    Comparacion(t, "Primera cuota", s.Francesa.PrimeraCuotaTotal, s.Alemana.PrimeraCuotaTotal);
                    Comparacion(t, "Última cuota", s.Francesa.UltimaCuotaTotal, s.Alemana.UltimaCuotaTotal);

                    Comparacion(t, "Total de intereses", s.Francesa.TotalInteres, s.Alemana.TotalInteres,
                        menorEsMejor: true);

                    if (s.IncluyeSeguroDesgravamen)
                    {
                        Comparacion(t, "Seguro de desgravamen", s.Francesa.TotalSeguro, s.Alemana.TotalSeguro,
                            menorEsMejor: true);
                    }

                    Comparacion(t, "Total a pagar", s.Francesa.TotalPagado, s.Alemana.TotalPagado,
                        menorEsMejor: true);
                    Comparacion(t, "Ingreso mínimo requerido",
                        s.Francesa.IngresoMinimoRequerido, s.Alemana.IngresoMinimoRequerido,
                        menorEsMejor: true);
                });

                col.Item().Background(AzulClaro).Padding(10).Text(Conclusion(s))
                    .FontSize(9.5f).FontColor(AzulOscuro);

                col.Item().Text(
                        $"El ingreso mínimo se calcula llevando la cuota más alta a su equivalente mensual " +
                        $"y dividiéndola para {s.RelacionCuotaIngreso.ToString("0.00", Cultura)}: la cuota no debería " +
                        $"comprometer más del {(s.RelacionCuotaIngreso * 100m).ToString("0", Cultura)} % de los ingresos.")
                    .FontSize(8.5f).FontColor(Gris);
            });
        });
    }

    /// <summary>Una página por método, con su tabla completa y los totales.</summary>
    private static void TablaMetodo(
        IDocumentContainer documento,
        SimulacionResponse s,
        TablaAmortizacion tabla,
        string titulo,
        string descripcion)
    {
        var conSeguro = s.IncluyeSeguroDesgravamen;

        documento.Page(pagina =>
        {
            Formato(pagina, s);

            pagina.Content().PaddingVertical(18).Column(col =>
            {
                col.Spacing(10);

                col.Item().Text(titulo).FontSize(16).Bold().FontColor(AzulOscuro);
                col.Item().Text(descripcion).FontSize(9).FontColor(Gris);

                col.Item().PaddingTop(4).Table(t =>
                {
                    t.ColumnsDefinition(c =>
                    {
                        // Ancho suficiente para que la palabra TOTAL de la fila
                        // de cierre no se parta en dos líneas.
                        c.ConstantColumn(46);
                        c.RelativeColumn();
                        c.RelativeColumn();
                        c.RelativeColumn();
                        if (conSeguro)
                        {
                            c.RelativeColumn();
                            c.RelativeColumn();
                        }
                        c.RelativeColumn();
                    });

                    // Se repite en cada página: una tabla de 240 filas ocupa
                    // varias hojas y sin esto se pierde qué significa cada columna.
                    t.Header(h =>
                    {
                        h.Cell().Element(Encabezado).AlignLeft().Text("N.º");
                        h.Cell().Element(Encabezado).AlignRight().Text("Capital");
                        h.Cell().Element(Encabezado).AlignRight().Text("Interés");
                        h.Cell().Element(Encabezado).AlignRight().Text(conSeguro ? "Cuota" : "Cuota total");
                        if (conSeguro)
                        {
                            h.Cell().Element(Encabezado).AlignRight().Text("Seguro");
                            h.Cell().Element(Encabezado).AlignRight().Text("Cuota total");
                        }
                        h.Cell().Element(Encabezado).AlignRight().Text("Saldo");
                    });

                    foreach (var fila in tabla.Cuotas)
                    {
                        // Filas alternas: con 240 líneas seguidas es fácil
                        // saltar de renglón al leer una cifra.
                        var fondo = fila.Periodo % 2 == 0 ? GrisClaro : Blanco;

                        t.Cell().Element(c => Celda(c, fondo)).AlignLeft()
                            .Text(fila.Periodo.ToString(Cultura)).FontColor(Gris);
                        t.Cell().Element(c => Celda(c, fondo)).AlignRight().Text(Moneda(fila.Capital));
                        t.Cell().Element(c => Celda(c, fondo)).AlignRight().Text(Moneda(fila.Interes));
                        t.Cell().Element(c => Celda(c, fondo)).AlignRight().Text(Moneda(fila.Cuota));

                        if (conSeguro)
                        {
                            t.Cell().Element(c => Celda(c, fondo)).AlignRight().Text(Moneda(fila.Seguro));
                            t.Cell().Element(c => Celda(c, fondo)).AlignRight()
                                .Text(Moneda(fila.CuotaTotal)).SemiBold();
                        }

                        t.Cell().Element(c => Celda(c, fondo)).AlignRight()
                            .Text(Moneda(fila.SaldoRestante)).FontColor(Gris);
                    }

                    t.Cell().Element(Total).AlignLeft().Text("TOTAL");
                    t.Cell().Element(Total).AlignRight().Text(Moneda(tabla.TotalCapital));
                    t.Cell().Element(Total).AlignRight().Text(Moneda(tabla.TotalInteres));
                    t.Cell().Element(Total).AlignRight().Text(Moneda(tabla.TotalCapital + tabla.TotalInteres));

                    if (conSeguro)
                    {
                        t.Cell().Element(Total).AlignRight().Text(Moneda(tabla.TotalSeguro));
                        t.Cell().Element(Total).AlignRight().Text(Moneda(tabla.TotalPagado));
                    }

                    t.Cell().Element(Total).AlignRight().Text("—");
                });
            });
        });
    }

    // ---------- Piezas compartidas ----------

    private static void Formato(PageDescriptor pagina, SimulacionResponse s)
    {
        pagina.Size(PageSizes.A4);
        pagina.Margin(1.6f, Unit.Centimetre);
        // Sin FontFamily: se usa la tipografia que QuestPDF incrusta, que existe
        // siempre. Nombrar una del sistema no garantiza que este instalada.
        pagina.DefaultTextStyle(x => x.FontSize(9));

        pagina.Header().BorderBottom(2).BorderColor(Azul).PaddingBottom(6).Row(fila =>
        {
            fila.RelativeItem().Column(c =>
            {
                c.Item().Text("Simulador de Créditos").FontSize(13).Bold().FontColor(AzulOscuro);
                c.Item().Text("Universidad Técnica de Ambato").FontSize(8.5f).FontColor(Gris);
            });

            fila.ConstantItem(200).AlignRight().Column(c =>
            {
                c.Item().Text(s.TipoCredito.Nombre).FontSize(10).SemiBold();
                c.Item().Text($"{Moneda(s.Monto)} · {s.PlazoMeses} meses · {Porcentaje(s.TasaAnualAplicada)}")
                    .FontSize(8.5f).FontColor(Gris);
            });
        });

        pagina.Footer().BorderTop(1).BorderColor(GrisClaro).PaddingTop(5).Row(fila =>
        {
            fila.RelativeItem().Text(
                    "Valores referenciales. No constituyen una oferta ni un compromiso de crédito.")
                .FontSize(7.5f).FontColor(Gris);

            fila.ConstantItem(120).AlignRight().Text(t =>
            {
                t.DefaultTextStyle(x => x.FontSize(7.5f).FontColor(Gris));
                t.Span("Página ");
                t.CurrentPageNumber();
                t.Span(" de ");
                t.TotalPages();
            });
        });
    }

    private static void Seccion(IContainer contenedor, string titulo) =>
        contenedor.PaddingTop(4).BorderBottom(1).BorderColor(Azul).PaddingBottom(3)
            .Text(titulo).FontSize(11).Bold().FontColor(Azul);

    private static void Dato(TableDescriptor t, string etiqueta, string valor)
    {
        t.Cell().PaddingVertical(3).Text(etiqueta).FontColor(Gris);
        t.Cell().PaddingVertical(3).AlignRight().Text(valor).SemiBold();
    }

    /// <summary>
    /// Fila del comparativo. Solo se resalta en verde cuando el valor menor es
    /// realmente el más conveniente: en las cuotas no lo es, porque una cuota
    /// baja suele venir acompañada de más intereses totales.
    /// </summary>
    private static void Comparacion(
        TableDescriptor t,
        string concepto,
        decimal frances,
        decimal aleman,
        bool menorEsMejor = false)
    {
        var mejorFrances = menorEsMejor && frances < aleman;
        var mejorAleman = menorEsMejor && aleman < frances;

        t.Cell().Element(c => Celda(c, Blanco)).AlignLeft().Text(concepto);
        t.Cell().Element(c => Celda(c, Blanco)).AlignRight()
            .Text(Moneda(frances)).SemiBold().FontColor(mejorFrances ? Colors.Green.Darken2 : Colors.Black);
        t.Cell().Element(c => Celda(c, Blanco)).AlignRight()
            .Text(Moneda(aleman)).SemiBold().FontColor(mejorAleman ? Colors.Green.Darken2 : Colors.Black);
    }

    private static IContainer Encabezado(IContainer c) =>
        c.Background(Azul).PaddingVertical(5).PaddingHorizontal(4)
            .DefaultTextStyle(x => x.FontColor(Colors.White).SemiBold().FontSize(8.5f));

    private static IContainer Celda(IContainer c, string fondo) =>
        c.Background(fondo).PaddingVertical(3).PaddingHorizontal(4);

    private static IContainer Total(IContainer c) =>
        c.Background(AzulClaro).PaddingVertical(5).PaddingHorizontal(4)
            .DefaultTextStyle(x => x.Bold().FontColor(AzulOscuro));

    private static string Conclusion(SimulacionResponse s)
    {
        if (s.NumeroCuotas == 1)
        {
            return "Con un pago único al vencimiento ambos métodos coinciden: se devuelve todo el capital " +
                   $"con sus intereses en una sola cuota de {Moneda(s.Francesa.TotalPagado)}.";
        }

        if (s.Comparativo.DiferenciaTotalInteres == 0m)
        {
            return "En este escenario ambos métodos generan el mismo total de intereses.";
        }

        if (s.Comparativo.MetodoMasEconomico == "Aleman")
        {
            var masAlta = s.Alemana.PrimeraCuotaTotal - s.Francesa.PrimeraCuotaTotal;
            return $"El método alemán ahorra {Moneda(s.Comparativo.DiferenciaTotalInteres)} en intereses. " +
                   $"A cambio, su primera cuota es {Moneda(masAlta)} más alta y exige mayor capacidad de pago " +
                   "al inicio. Si el presupuesto es ajustado, el francés ofrece una cuota más cómoda y predecible.";
        }

        return $"El método francés ahorra {Moneda(s.Comparativo.DiferenciaTotalInteres)} en intereses y " +
               "además mantiene la cuota fija durante todo el plazo.";
    }

    private static string Moneda(decimal valor) => valor.ToString("C2", Cultura);

    private static string Porcentaje(decimal valor, int decimales = 2) =>
        valor.ToString("N" + decimales, Cultura) + " %";
}
