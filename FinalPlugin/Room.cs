using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalPlugin
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class Room : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            Document doc = commandData.Application.ActiveUIDocument.Document;
            try
            {
                
                List<Level> levelList = new FilteredElementCollector(doc)  //создание списка уровней в модели
               .OfClass(typeof(Level))
               .OfType<Level>()
               .ToList();

                var roomTagType = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_RoomTags)
                    .OfType<RoomTagType>()
                    .Where(x => x.FamilyName.Equals("МаркаПомещения"))
                    .FirstOrDefault();


                if (roomTagType == null)  
                {
                    TaskDialog.Show("Ошибка", "Не найдено семейство \"МаркаПомещения\"");
                    return Result.Cancelled;
                }

                
                Transaction transaction1 = new Transaction(doc);  //активация семейства марки помещения
                transaction1.Start("Активация семейства \"МаркаПомещения\"");
                if (!roomTagType.IsActive)
                    roomTagType.Activate();
                transaction1.Commit();


                
                CreateRooms(doc, levelList);  //создание помещений
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Ошибка", ex.Message);
                return Result.Failed;
            }

            TaskDialog.Show("Сообщение", "Помещения промаркированы");
            return Result.Succeeded;
        }

        private static void CreateRooms(Document doc, List<Level> levelList)    //метод создающий помещения по уровням в модели
        {
            Transaction transaction2 = new Transaction(doc);
            transaction2.Start("Создание помещений");
            if (levelList.Count() > 0)
            {
                foreach (Level level in levelList)
                {
                    
                    PlanTopology planTopology = doc.get_PlanTopology(level);    //получаем план топологии выбранного уровня
                    if (planTopology != null)
                    {
                        
                        foreach (PlanCircuit circuit in planTopology.Circuits)
                        {
                            if (!circuit.IsRoomLocated)
                            {
                               doc.Create.NewRoom(null, circuit);
                            }                           

                        }
                    }

                }

            }
            transaction2.Commit();

        }
      
    }
}
