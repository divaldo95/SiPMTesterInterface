import React, { Component } from 'react';
import { Route, Routes } from 'react-router-dom';
import AppRoutes from './AppRoutes';
import { Layout } from './components/Layout';
import { MeasurementProvider } from './context/MeasurementContext';
import { LogProvider } from './context/LogContext'
import './custom.css';

export default class App extends Component {
  static displayName = App.name;

  render() {
      return (
          <MeasurementProvider>
              <LogProvider>
                  <Layout>
                      <Routes>
                          {AppRoutes.map((route, index) => {
                              const { element, ...rest } = route;
                              return <Route key={index} {...rest} element={element} />;
                          })}
                      </Routes>
                  </Layout>
              </LogProvider>
          </MeasurementProvider>
    );
  }
}
