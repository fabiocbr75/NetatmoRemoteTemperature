# DashboardReact

A modern React-based IoT dashboard for monitoring temperature sensors, replicating the functionality of the Angular Dashboard with improved performance and maintainability.

## Features

- **Real-time Dashboard**: Display sensor data from multiple temperature and weather sensors
- **Historical Data Visualization**: Interactive charts showing temperature, humidity, and valve data over time
- **Min/Max Analysis**: Statistical analysis of temperature data over selected time periods
- **Responsive Design**: Mobile-friendly interface that works on all devices
- **TypeScript**: Full type safety throughout the application
- **Modern Stack**: Built with React, Vite, Chart.js, and React Router

## Technology Stack

- **React 19** - UI framework
- **TypeScript** - Type safety
- **Vite** - Fast build tool and dev server
- **React Router** - Client-side routing
- **Chart.js** - Data visualization
- **Axios** - HTTP client
- **Playwright** - E2E testing

## Prerequisites

- Node.js (v18 or higher)
- npm (v9 or higher)

## Installation

1. Clone the repository:
```bash
git clone https://github.com/fabiocbr75/NetatmoRemoteTemperature.git
cd NetatmoRemoteTemperature/DashboardReact
```

2. Install dependencies:
```bash
npm install
```

3. Configure the API endpoint (optional):
Create a `.env` file in the root directory:
```
VITE_API_ENDPOINT=http://your-api-endpoint:5000/api
```

## Development

Start the development server:
```bash
npm run dev
```

The application will be available at `http://localhost:5173`

## Building for Production

Build the application:
```bash
npm run build
```

Preview the production build:
```bash
npm run preview
```

## Testing

Run Playwright tests:
```bash
npm run test
```

Run tests in UI mode:
```bash
npm run test:ui
```

Run tests in headed mode (see browser):
```bash
npm run test:headed
```

## Docker Deployment

Build the Docker image:
```bash
docker build -t dashboard-react .
```

Run the container:
```bash
docker run -d -p 8080:80 dashboard-react
```

The dashboard will be available at `http://localhost:8080`

## Project Structure

```
DashboardReact/
├── src/
│   ├── components/
│   │   ├── Dashboard/       # Dashboard page and components
│   │   │   ├── Dashboard.tsx
│   │   │   ├── TemperatureCard.tsx
│   │   │   └── WeatherCard.tsx
│   │   ├── History/         # History page and components
│   │   │   ├── History.tsx
│   │   │   ├── HistoryGraph.tsx
│   │   │   └── HistoryAnalysis.tsx
│   │   └── Layout/          # Layout components
│   │       └── Layout.tsx
│   ├── services/            # API services
│   │   └── api.service.ts
│   ├── types/               # TypeScript type definitions
│   │   └── sensor.types.ts
│   ├── App.tsx              # Main application component
│   ├── main.tsx             # Application entry point
│   └── index.css            # Global styles
├── tests/                   # Playwright E2E tests
│   ├── dashboard.spec.ts
│   ├── history.spec.ts
│   └── navigation.spec.ts
├── Dockerfile              # Docker configuration
├── playwright.config.ts    # Playwright configuration
└── vite.config.ts         # Vite configuration
```

## API Integration

The application connects to the TemperatureHub API backend. The default endpoint is `http://192.168.2.40:5000/api`, but this can be configured using the `VITE_API_ENDPOINT` environment variable.

### API Endpoints Used

- `GET /api/sensorMasterData` - Get all sensors
- `GET /api/sensorData/LastTemperature/{mac}` - Get latest temperature for a sensor
- `GET /api/sensorData/{mac}` - Get historical sensor data
- `GET /api/minMaxData4Day/{mac}` - Get min/max daily data
- `POST /api/sensorMasterData/SwitchPower/{mac}` - Toggle sensor power

## Features Comparison with Angular Dashboard

| Feature | Angular Dashboard | React Dashboard |
|---------|------------------|-----------------|
| Sensor Cards Display | ✅ | ✅ |
| Temperature Monitoring | ✅ | ✅ |
| Weather Display | ✅ | ✅ |
| Historical Charts | ✅ | ✅ |
| Min/Max Analysis | ✅ | ✅ |
| Date Range Selection | ✅ | ✅ |
| Power Toggle | ✅ | ✅ |
| Responsive Design | ✅ | ✅ |
| E2E Tests | Protractor | Playwright ✅ |

## Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new features
5. Submit a pull request

## License

MIT License - see LICENSE file for details

## Acknowledgments

- Based on the original Angular Dashboard by fabiocbr75
- Built with modern React best practices
- UI inspired by Nebular theme from the original dashboard
