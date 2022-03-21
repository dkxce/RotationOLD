// uses RaphaelJS

var AddDirectionMarker = function(mapalert,angle,onchangeDir)
{
	var marker = mapalert;
	marker.rotatedDiv = $(marker.div_);
	marker.rotatedDiv.html("");
    marker.Raphael = Raphael(marker.rotatedDiv[0], 32, 32);
    marker.rotatedImage = marker.Raphael.image("direction.png", 0, 0, 32, 32);
	marker.rotatedImage.fAngle = angle;
	marker.rotatedImage.move = false;
	marker.rotatedImage.onchangeDir = onchangeDir;
	marker.rotatedImage.rotate(marker.rotatedImage.fAngle);
	marker.setAngle = function(angle)
	{
		marker.rotatedImage.rotate(-1*marker.rotatedImage.fAngle);
		marker.rotatedImage.rotate(angle);
		marker.rotatedImage.fAngle = angle;
		if((onchangeDir !== undefined) && (onchangeDir != null)) onchangeDir(Math.floor(marker.rotatedImage.fAngle));
	};
	marker.rotatedDiv.mousedown(function(e) {marker.rotatedImage.move = true; return false;} );
	marker.rotatedDiv.mouseup(function(e) {marker.rotatedImage.move = false; return false;} );	
	marker.rotatedDiv.mousemove(function (e) {
			if(marker.rotatedImage.move)
			{				
				var x = e.pageX - (marker.rotatedDiv.offset().left + 16);
				var y = (marker.rotatedDiv.offset().top + 16) - e.pageY;
				if((x>-32)&&(x<32)&&(y>-32)&(y<32))
				{
					var atan = Math.atan(x/y)/Math.PI*180;
					if(y<0) atan = 180 + atan;
					if((x<0)&&(y>0)) atan = 360 + atan;
					marker.rotatedImage.rotate(-1*marker.rotatedImage.fAngle);
					marker.rotatedImage.rotate(atan);
					marker.rotatedImage.fAngle = atan;
					if((onchangeDir !== undefined) && (onchangeDir != null)) onchangeDir(Math.floor(atan));
					//marker.Raphael.clear();					
				};
			};
    }); 	
}