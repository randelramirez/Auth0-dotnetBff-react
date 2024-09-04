/* eslint-disable @typescript-eslint/no-explicit-any */
import { useEffect, useState } from "react";

export const FetchData = () => {
  const [forecasts, setForecasts] = useState([]);
  const [loading, setLoading] = useState(true);

  const fetchWeather = async () => {
    //TODO: refactor error handling later
    try {
      const response = await fetch("api/weatherforecast");

      if (response.status === 401) {
        window.location.href = "/auth/login";
      }

      if (response.status === 200) {
        const json = await response.json();

        setForecasts(json);
        setLoading(false);
      }
    } catch (error) {
      console.log(error);
    }
  };

  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const renderForecastsTable = function (forecasts:any) {
    return (
      <table className="table table-striped" aria-labelledby="tabelLabel">
        <thead>
          <tr>
            <th>Date</th>
            <th>Temp. (C)</th>
            <th>Temp. (F)</th>
            <th>Summary</th>
          </tr>
        </thead>
        <tbody>
          {forecasts.map((forecast:any) => (
            <tr key={forecast.date}>
              <td>{forecast.date}</td>
              <td>{forecast.temperatureC}</td>
              <td>{forecast.temperatureF}</td>
              <td>{forecast.summary}</td>
            </tr>
          ))}
        </tbody>
      </table>
    );
  };

  useEffect(() => {
    fetchWeather();
  }, []);

  const contents = loading ? (
    <p>
      <em>Loading...</em>
    </p>
  ) : (
    renderForecastsTable(forecasts)
  );

  return (
    <div>
      <h1 id="tabelLabel">Weather forecast</h1>
      <p>This component demonstrates fetching data from the server.</p>
      {contents}
    </div>
  );
};
