using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace invoiceCreator
{
    public class CustomerData
    {
        public string CustomerName { get; set; } // Kundenname
        public string Address { get; set; }      // Adresse
        public string City { get; set; }         // PLZ und Ort
        public string ProductName { get; set; }  // Produktname
        public int Quantity { get; set; }        // Stückzahl
        public decimal UnitPrice { get; set; }   // Textilpreis
        public decimal SetupFee { get; set; }    // Einrichtungsgebühr
        public decimal ProductionCost { get; set; } // Produktionskosten
        public decimal OverheadCost { get; set; }   // Overhead-Kosten
        public decimal ProjectTransfer { get; set; } // Projektübergabe
        public string CalculationMethod { get; set; } // Berechnungsmethode
        public decimal TotalPrice { get; set; }       // Gesamtpreis
        public DateTime Date { get; set; } = DateTime.Now; // Datum des Eintrags
    }



}
