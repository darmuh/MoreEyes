using System.Linq;

namespace MoreEyes
{
    //this wont be deleted and will persist until manually deleted
    //player selections wont need to be reloaded every time they spawn
    public class PlayerEyeSelection
    {
        internal static PlayerEyeSelection localSelections; //instance for our own selections
        //should save all selections to a text file in appdata probably
        internal string playerID = string.Empty;
        internal PatchedEyes patchedEyes;

        //Refs to current selections
        internal CustomPupilType pupilRight = CustomEyeManager.VanillaPupilRight;
        internal CustomPupilType pupilLeft = CustomEyeManager.VanillaPupilLeft;
        internal CustomIrisType irisRight = CustomEyeManager.VanillaIris;
        internal CustomIrisType irisLeft = CustomEyeManager.VanillaIris;
        public enum SelectionType
        {
            leftpupil,
            rightpupil,
            leftiris,
            rightiris
        }

        public PlayerEyeSelection(string steamID)
        {
            playerID = steamID;
            if (steamID == PlayerAvatar.instance.steamID)
                localSelections = this;
            CustomEyeManager.AllPlayerSelections.Add(this);
        }
        

        public void MakeSelection(SelectionType selectionType, string name)
        {
            //using type to determine pupil or iris (and left or right)

            if(selectionType == SelectionType.leftpupil)
            {
                CustomPupilType selection = CustomEyeManager.AllPupilTypes.FirstOrDefault(p => p.Name == name && p.AllowedPos != CustomEyeManager.Sides.Right);
                //if(selection != null)
                    //patchedEyes.SelectPupil(ref pupilLeft, selection, patchedEyes.LeftPupilObject, )
            }
            else if(selectionType == SelectionType.rightpupil)
            {
               pupilRight = CustomEyeManager.AllPupilTypes.FirstOrDefault(p => p.Name == name && p.AllowedPos != CustomEyeManager.Sides.Left);
            }
            else if(selectionType == SelectionType.leftiris)
            {
                irisLeft = CustomEyeManager.AllIrisTypes.FirstOrDefault(p => p.Name == name && p.AllowedPos != CustomEyeManager.Sides.Right);
            }
            else if (selectionType == SelectionType.rightiris)
            {
                irisLeft = CustomEyeManager.AllIrisTypes.FirstOrDefault(p => p.Name == name && p.AllowedPos != CustomEyeManager.Sides.Left);
            }
            else
            {
                Plugin.logger.LogError("Invalid selectionType at MakeSelection!!!");
            }
        }
    }
}
