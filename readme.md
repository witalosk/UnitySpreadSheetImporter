# Unity SpreadSheet Importer
A simple google spread sheet importer for unity.

You can import a google spread sheet and use it as a data source for your project.

## Installation
1. Open the Unity Package Manager
2. Click the + button
3. Select "Add package from git URL..."
4. Enter `https://github.com/witalosk/UnitySpreadSheetImporter.git?path=Packages/com.witalosk.spreadsheetimporter`

## Usage
1. Open your Google SpreadSheet
2. Click "Share" button
3. Click "Get shareable link"
4. Get the id from the link (e.g. https://docs.google.com/spreadsheets/d/1X2Y3Z/edit#gid=0, the id is 1X2Y3Z)
5. Get the sheet gid (e.g. https://docs.google.com/spreadsheets/d/1X2Y3Z/edit#gid=123456, the gid is 123456)
6. Use the `SpreadSheetImporter` class to import the data

```csharp
    public class TestScript : MonoBehaviour
    {
        [SerializeField] private string _spreadSheetId = "1X2Y3Z";
        [SerializeField] private string _sheetGId = "123456";
        [SerializeField] private int _headerRow = 1;
        [SerializeField] private int _dataStartRow = 2;
        [SerializeField] private int _primaryKeyColumn = 1;
        
        private async void Start()
        {
            var data = await SpreadSheetImporter.GetSpreadSheetDataAsync(_spreadSheetId, _sheetGId, _headerRow, _dataStartRow, _primaryKeyColumn);
            
            foreach (var row in data)
            {
                Debug.Log($"[{row["Name"]}]: Price:{row["Price"]}, Note:{row["Note"]}");
            }
        }
    }
```