//Script_VehicleLocking by ZombieDude

//Error
%error = forceRequiredAddon("Item_Key");
if(%error == $Error::AddOn_NotFound)
	error("Script_VehicleLocking : Required addon Item_Key not found!");

//Executing
exec("./carkey.cs");

//Preferences, for Insert Name Here

if(isFile("add-ons/system_returntoblockland/server.cs")) {
	if(!$RTB::Hooks::ServerControl)
		exec("Add-Ons/System_ReturnToBlockland/hooks/ServerControl.cs");
	RTB_registerPref("Admin Unlock?", "VehicleLocking", "Pref::Merp::NameCharLimit", "list SuperAdmin 1 Admins 2 Host 3", "Server_VehicleLocking", 75, 0, 0);
	RTB_registerPref("Admin Level Required", "VehicleLocking", "Pref::Merp::DefaultRNum", "bool", "Server_VehicleLocking", 1, 0, 0);
} 
else {
	if($Pref::VLocking::AdminAllowance	$= "")	$Pref::VLocking::AdminAllowance = true;
	if($Pref::VLocking::AdminLevel	$= "")	$Pref::VLocking::AdminAllowance = "Super Admin";
}

//Packages
package vehicleLocking {
	function fxDTSBrick::setVehicle(%brick, %vehicle) {
		parent::setVehicle(%brick, %vehicle);
		%client = %brick.getGroup().client;
	}
	function WheeledVehicleData::onCollision(%this, %obj, %col, %pos, %vel) {
		if(%obj.locked && %col.getType() & $TypeMasks::PlayerObjectType && isObject(%col.client) && getTrustLevel(%col.client.brickGroup,findClientByBl_ID(%obj.owner)) < 3){
			commandToClient(%col.client, 'centerPrint', "\c6This\c3"SPC trim(%obj.getDatablock().uiname) SPC"\c6is locked!", 3);
			return;
		}
		else if(!%obj.owner)
			%obj.owner = %col.client.bl_id;
		parent::onCollision(%this, %obj, %col, %pos, %vel);
	}
	function FlyingVehicleData::onCollision(%this, %obj, %col, %pos, %vel) {
		if(%obj.locked && %col.getType() & $TypeMasks::PlayerObjectType && isObject(%col.client)){
			commandToClient(%col.client, 'centerPrint', "\c6This\c3"SPC trim(%obj.getDatablock().uiname) SPC"\c6is locked!", 3);
			return;
		}
		else if(!%obj.owner)
			%obj.owner = %col.client.bl_id;
		parent::onCollision(%this, %obj, %col, %pos, %vel);
	}

};
activatePackage(vehicleLocking);

//Server commands
function serverCmdtogglelock(%c) {
	%startp = %c.player.getEyePoint();
	%ev = %c.player.getEyeVector();
	%mask = $TypeMasks::VehicleObjectType;

	%raycast = containerRaycast(%startp, VectorAdd(%startp,VectorScale(%ev,30)),%mask);
	if(!%raycast)
		commandToClient(%c, 'centerPrint', "\c6That isn't a vehicle!", 3);

	%vehicle = getWord(%raycast,0);
	if(!isObject(%vehicle))
		return;
	if(%vehicle.owner != %c.bl_id)
		commandToClient(%c, 'centerPrint', "\c6You do not own that\c3"SPC trim(%vehicle.getDatablock().uiname) SPC"\c6.", 3);
	else if(%vehicle.owner == %c.bl_id && isObject(%c)){
		%vehicle.locked = !%vehicle.locked;
		commandToClient(%c, 'centerPrint', "\c6Your\c3"SPC trim(%vehicle.getDatablock().uiname) SPC "\c6is now \c3" @ (%vehicle.locked ? "locked":"unlocked")@ "\c6.", 3);
	}
	else
		commandToClient(%c,'centerPrint',"\c6That\c3"SPC trim(%vehicle.getDatablock().uiname) SPC"\c6does not have an owner! Drive it to claim it.");

}