/* eslint-disable @typescript-eslint/no-explicit-any */
import React, { useState, useEffect, useMemo } from "react";

// context value does not get populated
export const AuthContext = React.createContext<any>({
  isAuthenticated: false,
  user: null,
  isLoading: false,
  login: () => {},
  logout: () => {},
});

// export const useAuth = () => useContext(AuthContext);
export const AuthProvider = ({ children }: any) => {
  const [isAuthenticated, setIsAuthenticated] = useState();
  const [user, setUser] = useState();
  const [isLoading, setIsLoading] = useState(false);

  const getUser = async () => {
    try {
      const response = await fetch("/auth/GetUser");
      if (response.ok) {
        const json = await response.json();
        console.log({json})
        setIsAuthenticated(json.isAuthenticated);
        setIsLoading(false);
        if (json.isAuthenticated) {
          console.log("claims", json.claims);
          setUser(json.claims);
        }
      }
    } catch (error) {
      // debugger;
      console.error(error);
    }
  };

  useEffect(() => {
    console.log('get user..')
    getUser();
  }, []);

  const login = () => {
    // eslint-disable-next-line no-debugger
    // debugger;
    console.log("login called");
    window.location.href = "/auth/login";
  };

  const logout = () => {
    window.location.href = "/auth/logout";
  };

  const context = useMemo(
    () => ({
      isAuthenticated,
      user,
      isLoading,
      login,
      logout,
    }),
    [isAuthenticated, isLoading, user]
  );
console.log('AuthContext value', context);
  return (
    <AuthContext.Provider value={context}>{children}</AuthContext.Provider>
  );
};
