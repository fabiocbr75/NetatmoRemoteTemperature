import { NbMenuItem } from '@nebular/theme';

export const MENU_ITEMS: NbMenuItem[] = [
  {
    title: 'IoT Dashboard',
    icon: 'nb-home',
    link: '/pages/iot-dashboard',
    home: true,
  },
  {
    title: 'DETAILS',
    group: true,
  },
  {
    title: 'Rooms',
    icon: 'nb-star',
    children: [
      {
        title: 'Cucina',
        link: '/pages/extra-components/calendar',
      },
      {
        title: 'Sala',
        link: '/pages/extra-components/stepper',
      },
      {
        title: 'Camera',
        link: '/pages/extra-components/list',
      },
      {
        title: 'Bagno',
        link: '/pages/extra-components/infinite-list',
      },
      {
        title: 'Cameretta',
        link: '/pages/extra-components/accordion',
      },
      {
        title: 'Studio',
        link: '/pages/extra-components/progress-bar',
      },
    ],
  },
];
