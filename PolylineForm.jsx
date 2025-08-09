import React, { useState } from "react";
import PolylineMap from "./PolylineMap";
import "./styles.css";

export default function PolylineForm() {
  const [polylineCoords, setPolylineCoords] = useState([]);
  const [lat, setLat] = useState("");
  const [lng, setLng] = useState("");
  const [error, setError] = useState(null);

  // Format coordinates for display
  const formatCoordinates = (coords) =>
    coords
      .map((ll) => `(${ll[0].toFixed(4)}, ${ll[1].toFixed(4)})`)
      .join(", ");

  // Handle form submission for adding points
  const handleAddPoint = (e) => {
    e.preventDefault();
    const latitude = parseFloat(lat);
    const longitude = parseFloat(lng);
    if (isNaN(latitude) || isNaN(longitude)) {
      setError(new Error("Please enter valid latitude and longitude values."));
      return;
    }
    setPolylineCoords([...polylineCoords, [latitude, longitude]]);
    setLat("");
    setLng("");
    setError(null);
  };

  // Handle form submission for sending coordinates
  const handleSubmit = (e) => {
    e.preventDefault();
    if (polylineCoords.length < 2) {
      setError(new Error("At least two points are required for a polyline."));
      return;
    }
    // Simulate sending data to a server or logging it
    console.log("Submitted coordinates:", polylineCoords);
    alert(`Submitted: ${formatCoordinates(polylineCoords)}`);
    setPolylineCoords([]);
    setError(null);
  };

  return (
    <div className="container">
      {error && <div className="error">Error: {error.message}</div>}

      <PolylineMap
        polylineCoords={polylineCoords}
        setPolylineCoords={setPolylineCoords}
        setError={setError}
      />

      <form onSubmit={handleAddPoint} className="point-form">
        <label>
          Latitude:
          <input
            type="number"
            step="any"
            value={lat}
            onChange={(e) => setLat(e.target.value)}
            placeholder="e.g., 51.505"
            aria-label="Latitude"
          />
        </label>
        <label>
          Longitude:
          <input
            type="number"
            step="any"
            value={lng}
            onChange={(e) => setLng(e.target.value)}
            placeholder="e.g., -0.09"
            aria-label="Longitude"
          />
        </label>
        <button type="submit" aria-label="Add point to polyline">
          Add Point
        </button>
      </form>

      <textarea
        rows="4"
        className="textarea"
        value={formatCoordinates(polylineCoords)}
        readOnly
        aria-label="Polyline coordinates"
        onClick={(e) => {
          e.target.select();
          document.execCommand("copy");
        }}
      />

      <form onSubmit={handleSubmit} className="submit-form">
        <button type="submit" aria-label="Submit polyline coordinates">
          Submit Polyline
        </button>
      </form>
    </div>
  );
}
