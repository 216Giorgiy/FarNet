import FarManager
import System.Reflection
public class JSCalc extends BasePlugin{
	function item_OnOpen(sender:Object, e:OpenPluginMenuItemEventArgs) {
		var InpBox = Far.CreateInputBox();
		InpBox.Prompt = "Enter expression";
		InpBox.History = Assembly.GetExecutingAssembly().Location;
		if(InpBox.Show()){
			var Result = 0;
			eval("Result=" + InpBox.Text);
			Far.Msg( "Result=" + Result );
		};
	}
	function Connect(){
		var menuItem:IPluginMenuItem=Far.CreatePluginsMenuItem();
		menuItem.Name="Hello js.net";
		menuItem.add_OnOpen(this.item_OnOpen);
		Far.RegisterPluginsMenuItem(menuItem);
	}
}
