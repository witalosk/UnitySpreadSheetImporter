using UnityEngine;

namespace SpreadSheetImporter.Sample
{
    public class TestScript : MonoBehaviour
    {
        [SerializeField] private string _spreadSheetId = "1ViODE7UGRc-wUU1GNuiW8qP3AyFqnP1YZv163lyGqSo";
        [SerializeField] private string _sheetGId = "0";
        [SerializeField] private int _headerRow = 1;
        [SerializeField] private int _dataStartRow = 2;
        [SerializeField] private int _primaryKeyColumn = 1;
        
        private async void Start()
        {
            var data = await SpreadSheetImporter.GetSpreadSheetDataAsync(_spreadSheetId, _sheetGId, _headerRow, _dataStartRow, _primaryKeyColumn);
            
            foreach (var row in data.Rows)
            {
                Debug.Log($"[{row["Name"]}]: Price:{row["Price"]}, Note:{row["Note"]}");
            }
        }
    }
}