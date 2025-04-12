# Gotorz Travel Booking Application

This application allows users to search for and book travel packages that include flights and hotels using the Amadeus API.

## Amadeus API Configuration

To use the Amadeus flight and hotel APIs, you need to set up your API credentials:

1. Register for an Amadeus for Developers account at https://developers.amadeus.com/
2. Create a new application to get your API Key and API Secret
3. Update the `appsettings.json` file with your credentials:

```json
"AmadeusAPI": {
  "BaseUrl": "https://test.api.amadeus.com/v1", // Use https://api.amadeus.com/v1 for production
  "TokenUrl": "https://test.api.amadeus.com/v1/security/oauth2/token",
  "ClientId": "YOUR_AMADEUS_API_KEY",
  "ClientSecret": "YOUR_AMADEUS_API_SECRET",
  "FlightOffersUrl": "https://test.api.amadeus.com/v2/shopping/flight-offers",
  "HotelOffersUrl": "https://test.api.amadeus.com/v3/shopping/hotel-offers",
  "HotelsByCityUrl": "https://test.api.amadeus.com/v1/reference-data/locations/hotels/by-city",
  "AirportAndCitySearchUrl": "https://test.api.amadeus.com/v1/reference-data/locations"
}
```

## Application Features

The application provides the following features:

1. **Flight Search**: Search for flights by origin, destination, date, and number of adults
2. **Hotel Search**: Search for hotels by city, check-in/check-out dates, and number of adults
3. **Travel Package Creation**: Combine flights and hotels into a travel package

## API Services

The application uses the following Amadeus API services:

1. **Flight Offers Search** (`GET /v2/shopping/flight-offers`): Search for flight offers
2. **Hotel Offers Search** (`GET /v3/shopping/hotel-offers`): Search for hotel offers by hotel IDs
3. **Hotels by City** (`GET /v1/reference-data/locations/hotels/by-city`): Get a list of hotel IDs in a city
4. **Airport and City Search** (`GET /v1/reference-data/locations`): Search for airports and cities by name

## Development Notes

- The application uses a local JSON file to cache airport data to reduce API calls
- Authorization is handled via OAuth2 with client credentials flow
- API responses are logged for debugging purposes
- The application includes error handling for common API issues

## Troubleshooting

- If you encounter authentication errors, verify your API Key and Secret are correctly set in `appsettings.json`
- If no results are returned for flights or hotels, check the console logs for detailed error messages
- Make sure you're using valid IATA codes for airports and cities

## Testing the Application

1. Enter a valid origin and destination airport (e.g., "PAR" for Paris, "LON" for London)
2. Select valid dates for your travel
3. Click "Search Flights" to find available flights
4. Select a flight and then search for hotels
5. Create a travel package by selecting both a flight and hotel
