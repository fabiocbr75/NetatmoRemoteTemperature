# DashboardReact - Development Guide

## Overview

DashboardReact is a modern React-based IoT dashboard that provides real-time monitoring and historical data visualization for temperature sensors. It's designed to replace the existing Angular dashboard with improved performance and maintainability.

## Architecture

### Component Structure

```
DashboardReact/
├── src/
│   ├── components/
│   │   ├── Dashboard/           # Main dashboard view
│   │   │   ├── Dashboard.tsx    # Container for sensor cards
│   │   │   ├── TemperatureCard.tsx  # Internal sensor display
│   │   │   └── WeatherCard.tsx      # External sensor display
│   │   ├── History/             # Historical data view
│   │   │   ├── History.tsx      # Container with date controls
│   │   │   ├── HistoryGraph.tsx     # Chart.js line chart
│   │   │   └── HistoryAnalysis.tsx  # Min/Max table view
│   │   └── Layout/              # App layout wrapper
│   │       └── Layout.tsx       # Header and navigation
│   ├── services/                # API integration layer
│   │   └── api.service.ts       # HTTP client and endpoints
│   ├── types/                   # TypeScript definitions
│   │   └── sensor.types.ts      # Data models
│   └── App.tsx                  # Root component with routing
```

### State Management

The application uses React's built-in state management:
- **Local State**: Component-level state with `useState`
- **Side Effects**: Data fetching with `useEffect` and `useCallback`
- **No Redux**: Simplified architecture without global state library

### API Integration

All API calls are centralized in `src/services/api.service.ts`:

```typescript
// Example usage
import { sensorService, temperatureService } from './services/api.service';

// Fetch all sensors
const sensors = await sensorService.getSensorMasterData();

// Get temperature data
const temp = await temperatureService.getLastTemperature(mac);
```

## Development Workflow

### Local Development

1. Start the dev server:
```bash
npm run dev
```

2. The app runs at `http://localhost:5173` with hot module replacement

3. Edit components in `src/components/` and see changes instantly

### Code Quality

Before committing, ensure:

```bash
# Check for linting errors
npm run lint

# Build the project
npm run build

# Run tests
npm run test
```

### Adding New Components

1. Create component file in appropriate directory:
```typescript
// src/components/NewComponent/NewComponent.tsx
import './NewComponent.css';

interface NewComponentProps {
  // Define props
}

const NewComponent = ({ ...props }: NewComponentProps) => {
  return (
    <div className="new-component">
      {/* Component JSX */}
    </div>
  );
};

export default NewComponent;
```

2. Create corresponding CSS file:
```css
/* src/components/NewComponent/NewComponent.css */
.new-component {
  /* Styles */
}
```

3. Import and use in parent component

### Adding New API Endpoints

1. Define types in `src/types/sensor.types.ts`:
```typescript
export interface NewDataType {
  field1: string;
  field2: number;
}
```

2. Add service method in `src/services/api.service.ts`:
```typescript
export const newService = {
  getData: async (): Promise<NewDataType> => {
    const response = await apiClient.get<NewDataType>('/endpoint');
    return response.data;
  },
};
```

3. Use in components:
```typescript
import { newService } from '../../services/api.service';

const data = await newService.getData();
```

## Testing

### Writing Tests

Tests use Playwright for E2E testing. Example:

```typescript
// tests/new-feature.spec.ts
import { test, expect } from '@playwright/test';

test.describe('New Feature', () => {
  test('should work correctly', async ({ page }) => {
    await page.goto('/');
    await expect(page.locator('.element')).toBeVisible();
  });
});
```

### Running Tests

```bash
# Run all tests
npm run test

# Run tests with UI
npm run test:ui

# Run tests in headed mode (see browser)
npm run test:headed
```

## Deployment

### Docker Deployment

Build and run with Docker:

```bash
# Build image
docker build -t dashboard-react .

# Run container
docker run -d -p 8080:80 dashboard-react
```

### Manual Deployment

1. Build the production bundle:
```bash
npm run build
```

2. The `dist/` folder contains static files ready for deployment

3. Serve with any static file server (nginx, Apache, etc.)

### Environment Variables

Configure the API endpoint using environment variables:

```bash
# .env file
VITE_API_ENDPOINT=http://your-api-server:5000/api
```

## Common Issues

### API Connection Errors

**Problem**: Can't connect to backend API

**Solution**: 
- Check API endpoint in `.env` file
- Ensure backend is running and accessible
- Check CORS settings on backend

### Build Errors

**Problem**: TypeScript compilation errors

**Solution**:
- Run `npm run lint` to see all errors
- Check type definitions in `src/types/`
- Ensure all dependencies are installed

### Chart Not Rendering

**Problem**: History graph shows blank

**Solution**:
- Ensure data is being fetched (check network tab)
- Verify data format matches expected structure
- Check console for Chart.js errors

## Performance Optimization

### Bundle Size

Current production bundle:
- Total: ~484 KB
- Gzipped: ~158 KB

### Optimization Tips

1. **Code Splitting**: Already handled by Vite
2. **Lazy Loading**: Can add for routes if needed
3. **Image Optimization**: Use WebP format for images
4. **Caching**: Configure service worker for offline support

## Browser Support

- Chrome/Edge (latest 2 versions)
- Firefox (latest 2 versions)
- Safari (latest 2 versions)
- Mobile browsers (iOS Safari, Chrome Mobile)

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make changes with tests
4. Run linting and tests
5. Submit pull request

## Resources

- [React Documentation](https://react.dev)
- [Vite Documentation](https://vitejs.dev)
- [Chart.js Documentation](https://www.chartjs.org)
- [Playwright Documentation](https://playwright.dev)
- [TypeScript Documentation](https://www.typescriptlang.org)
