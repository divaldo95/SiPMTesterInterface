﻿import React, { createContext, useState } from 'react';
import MeasurementStateService from '../services/MeasurementStateService';

// Create the context
export const LogContext = createContext();

// Create a provider component
export const LogProvider = ({ children }) => {
    const [logs, setLogs] = useState([]);
    const [unresolvedLogsFetched, setUnresolvedLogsFetched] = useState([]);
    const [attentionNeededLogsFetched, setAttentionNeededLogsFetched] = useState([]);

    const fetchLogs = async () => {
        try {
            const data = await MeasurementStateService.getAllLogs()
                .then((resp) => {
                    setLogs(resp);
                    console.log(resp);
                })
        } catch (error) {
            console.error('Error fetching log messages:', error);
        }
    }

    const fetchUnresolvedLogs = async () => {
        try {
            const data = await MeasurementStateService.getUnresolvedLogs()
                .then((resp) => {
                    setUnresolvedLogsFetched(resp);
                    console.log(resp);
                })
        } catch (error) {
            console.error('Error fetching log messages:', error);
        }
    }

    const fetchNeedsAttentiondLogs = async () => {
        try {
            const data = await MeasurementStateService.getAttentionNeededLogs()
                .then((resp) => {
                    setAttentionNeededLogsFetched(resp);
                    console.log(resp);
                })
        } catch (error) {
            console.error('Error fetching log messages:', error);
        }
    }

    const unresolvedLogs = logs.filter(log => !log.Resolved);

    const getAttentionNeededLogs = logs.filter(log => log.NeedsAttention);

    const updateLogsResolved = (id) => {
        setLogs((prevLogs) =>
            prevLogs.map((log) =>
                log.ID === id ? { ...log, Resolved: true } : log
            )
        );
        console.log(logs);
    };

    const appendLog = (newLog) => {
        setLogs(prevLogs => {
            const existingLog = prevLogs.find(log => log.ID === newLog.ID);
            if (!existingLog) {
                return [...prevLogs, newLog];
            }
            return prevLogs;
        });
    };

    return (
        <LogContext.Provider value={{
            logs, unresolvedLogs, attentionNeededLogsFetched, fetchLogs,
            fetchNeedsAttentiondLogs, updateLogsResolved,
            getAttentionNeededLogs, unresolvedLogs, appendLog
        }}>
            {children}
        </LogContext.Provider>
    );
};