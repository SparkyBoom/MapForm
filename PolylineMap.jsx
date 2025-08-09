import React, { useState, useRef, useEffect } from "react";
import { MapContainer, TileLayer, Polyline } from "react-leaflet";
import { EditControl } from "react-leaflet-draw";
import L from "leaflet";
import icon from "leaflet/dist/images/marker-icon.png";
import iconShadow from "leaflet/dist/images/marker-shadow.png";
import "leaflet/dist/leaflet.css";
import "leaflet-draw/dist/leaflet.draw.css";
import "./styles.css";

// Fix Leaflet icon paths
const DefaultIcon = L.icon({
  iconUrl: icon,
  shadowUrl: iconShadow,
});
L.Marker.prototype.options.icon = DefaultIcon;

export default function PolylineMap({
  polylineCoords,
  setPolylineCoords,
  setError,
}) {
  const [drawEnabled, setDrawEnabled] = useState(false);
  const drawRef = useRef(null);
  const mapRef = useRef(null);

  // Handle polyline creation from map drawing
  const onCreated = (e) => {
    try {
      if (e.layerType === "polyline") {
        const latlngs = e.layer.getLatLngs();
        setPolylineCoords(latlngs.map((ll) => [ll.lat, ll.lng]));
      }
    } catch (err) {
      setError(err);
    }
  };

  // Toggle drawing mode
  const toggleDraw = (enable) => {
    setDrawEnabled(enable);
    if (drawRef.current) {
      const drawControl = drawRef.current._leaflet_draw;
      if (enable) {
        drawControl._toolbars.draw._modes.polyline.handler.enable();
      } else {
        drawControl._toolbars.draw._modes.polyline.handler.disable();
      }
    }
  };

  // Reset map
  const resetMap = () => {
    setPolylineCoords([]);
    if (mapRef.current) {
      mapRef.current.eachLayer((layer) => {
        if (layer instanceof L.Polyline) {
          mapRef.current.removeLayer(layer);
        }
      });
    }
  };

  // Cleanup on unmount
  useEffect(() => {
    return () => {
      if (mapRef.current) {
        mapRef.current.remove();
      }
    };
  }, []);

  return (
    <div className="map-section">
      <div className="button-group">
        <button onClick={() => toggleDraw(true)} aria-label="Start drawing a polyline">
          Draw Polyline
        </button>
        <button onClick={() => toggleDraw(false)} aria-label="Finish drawing">
          Finish
        </button>
        <button onClick={resetMap} aria-label="Reset map">
          Reset Map
        </button>
      </div>

      <MapContainer
        center={[51.505, -0.09]}
        zoom={13}
        className="map-container"
        ref={mapRef}
      >
        <TileLayer
          url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
          attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
        />
        {polylineCoords.length > 0 && (
          <Polyline positions={polylineCoords} color="blue" />
        )}
        <EditControl
          ref={drawRef}
          position="topright"
          onCreated={onCreated}
          draw={{
            polyline: drawEnabled,
            polygon: false,
            circle: false,
            rectangle: false,
            marker: false,
            circlemarker: false,
          }}
        />
      </MapContainer>
    </div>
  );
}
