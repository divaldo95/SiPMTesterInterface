import { Counter } from "./components/Counter";
import { FetchData } from "./components/FetchData";
import { Home } from "./components/Home";
import Measurement from "./components/Measurement"

const AppRoutes = [
  {
    index: true,
    element: <Home />
  },
  {
    path: '/counter',
    element: <Counter />
    },
    {
        path: '/sipm',
        element: <Measurement />
    },
  {
    path: '/fetch-data',
    element: <FetchData />
  }
];

export default AppRoutes;
