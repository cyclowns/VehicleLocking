//Car Key datablocks/stuff

datablock ItemData(carKeyItem)
{
	 // Basic Item Properties
	shapeFile = "Add-Ons/Item_Key/keyA.dts";
	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;

	//gui properties
	uiName = "Car Key";
	iconName = "Add-Ons/Item_Key/Icon_KeyA";
	doColorShift = true;
	colorShiftColor = "1.0 0.0 0.0 1.0";

	 // Dynamic properties defined by the scripts
	image = carKeyImage;
	canDrop = true;
};

datablock ShapeBaseImageData(carKeyImage)
{
	 // Basic Item properties
	 shapeFile = "Add-Ons/Item_Key/keyA.dts";
	 emap = true;

	 // Specify mount point & offset for 3rd person, and eye offset
	 // for first person rendering.
	 mountPoint = 0;
	 offset = "0 0 0";

	 // When firing from a point offset from the eye, muzzle correction
	 // will adjust the muzzle vector to point to the eye LOS point.
	 // Since this weapon doesn't actually fire from the muzzle point,
	 // we need to turn this off.  
	 correctMuzzleVector = false;

	 // Add the WeaponImage namespace as a parent, WeaponImage namespace
	 // provides some hooks into the inventory system.
	 className = "WeaponImage";

	 // Projectile && Ammo.
	 item = carKeyItem;
	 ammo = " ";
	 projectile = "";
	 projectileType = "";

	 //melee particles shoot from eye node for consistancy
	 melee = true;
	 doRetraction = false;
	 //raise your arm up or not
	 armReady = true;

	 showBricks = false;

	 //casing = " ";

	 doColorShift = true;
	 colorShiftColor = carKeyItem.colorShiftColor;

	 // Images have a state system which controls how the animations
	 // are run, which sounds are played, script callbacks, etc. This
	 // state system is downloaded to the client so that clients can
	 // predict state changes and animate accordingly.  The following
	 // system supports basic ready->fire->reload transitions as
	 // well as a no-ammo->dryfire idle state.

	 // Initial start up state
	stateName[0]                     = "Activate";
	stateTimeoutValue[0]             = 0.0;
	stateTransitionOnTimeout[0]      = "Ready";

	stateName[1]                     = "Ready";
	stateTransitionOnTriggerDown[1]  = "PreFire";
	stateAllowImageChange[1]         = true;

	stateName[2]                    = "PreFire";
	stateScript[2]                  = "onPreFire";
	stateAllowImageChange[2]        = true;
	stateTimeoutValue[2]            = 0.01;
	stateTransitionOnTimeout[2]     = "Fire";

	stateName[3]                    = "Fire";
	stateTransitionOnTimeout[3]     = "CheckFire";
	stateTimeoutValue[3]            = 0.15;
	stateFire[3]                    = true;
	stateAllowImageChange[3]        = true;
	stateSequence[3]                = "Fire";
	stateScript[3]                  = "onFire";
	stateWaitForTimeout[3]           = true;
	stateSequence[3]                = "Fire";

	stateName[4]                    = "CheckFire";
	stateTransitionOnTriggerUp[4]   = "StopFire";

	stateName[5]                    = "StopFire";
	stateTransitionOnTimeout[5]     = "Ready";
	stateTimeoutValue[5]            = 0.01;
	stateAllowImageChange[5]        = true;
	stateWaitForTimeout[5]          = true;
	stateSequence[5]                = "StopFire";
	stateScript[5]                  = "onStopFire";
};

function carKeyImage::onPreFire(%this, %obj, %slot)
{
	%obj.playthread(2, shiftLeft);
}

function carKeyImage::onStopFire(%this, %obj, %slot)
{   
	%obj.playthread(2, root);
}

function carKeyImage::onFire(%this, %player, %slot)
{
	%start = %player.getEyePoint();
	%vec = vectorScale(%player.getMuzzleVector(%slot), 10  * getWord(%player.getScale(), 2) );
	%end = vectorAdd(%start, %vec);
	%mask = $TypeMasks::VehicleObjectType;

	%rayCast = containerRayCast(%start,%end,%mask);

	if(!%rayCast)
	 return;

	%hitObj = getWord(%rayCast, 0);
	%hitPos = getWords(%rayCast, 1, 3);
	%hitNormal = getWords(%rayCast, 4, 6);

	%this.onHitObject(%player, %slot, %hitObj, %hitPos, %hitNormal);   
}
function carKeyImage::onHitObject(%this, %player, %slot, %hitObj, %hitPos, %hitNormal)
{
	%client = %player.client;
	if(%hitObj.getType() & $TypeMasks::VehicleObjectType){
		if($Pref::VLocking::AdminAllowance){
			switch($Pref::VLocking::AdminLevel){
				case 1: %aa = %client.isAdmin;
				case 2: %aa = %client.isSuperAdmin;
				case 3: %aa = getNumKeyID() == %client.bl_id;
			}
		}
		if(%hitObj.owner == %client.bl_id || %client.isSuperAdmin){
			%hitObj.locked = !%hitObj.locked;
			commandToClient(%client, 'centerPrint', "\c6Your\c3"SPC trim(%hitObj.getDatablock().uiname) SPC "\c6is now \c3" @ (%hitObj.locked ? "locked":"unlocked")@ "\c6.", 3);
		}
		else{
			commandToClient(%client, 'centerPrint', "\c6You do not own that\c3"SPC trim(%vehicle.getDatablock().uiname) SPC"\c6.", 3);
		}
	}
}