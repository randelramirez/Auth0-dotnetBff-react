/* eslint-disable @typescript-eslint/no-explicit-any */
/* eslint-disable @typescript-eslint/no-unused-vars */
import { useState } from 'react';
import { Collapse, Container, Navbar, NavbarBrand, NavbarToggler, NavItem, NavLink } from 'reactstrap';
import { Link } from 'react-router-dom';
// import { useAuth } from '../context/AuthContext';

import './NavMenu.css';
import { useAuth } from '../context/useAuth';

export const NavMenu = (props:any) => {
    // login and logout are not handled by react router
    const { isAuthenticated, login, logout } = useAuth();

    const [collapsed, setCollapsed] = useState(true);

    const toggleNavbar = () => {
        setCollapsed(!collapsed);
    }

    return (
        <header>
            <Navbar className="navbar-expand-sm navbar-toggleable-sm ng-white border-bottom box-shadow mb-3" light>
                <Container >
                    <NavbarBrand tag={Link} to="/">Auth0 Backend For Frontend Authentication</NavbarBrand>
                    <NavbarToggler onClick={toggleNavbar} className="mr-2" />
                    <Collapse className="d-sm-inline-flex flex-sm-row-reverse" isOpen={!collapsed} navbar>
                        <ul className="navbar-nav flex-grow">
                            <NavItem>
                                <NavLink tag={Link} className="text-dark" to="/">Home</NavLink>
                            </NavItem>
                            <NavItem>
                                <NavLink tag={Link} className="text-dark" to="/fetch-data">Fetch data</NavLink>
                            </NavItem>
                            <NavItem>
                                <NavLink tag={Link} className="text-dark" to="/user">User claims</NavLink>
                            </NavItem>
                            {!isAuthenticated && <NavItem>
                                <NavLink tag={Link} className="text-dark" onClick={()=> {login();}}>Login</NavLink>
                            </NavItem>}
                            {isAuthenticated && <NavItem>
                                <NavLink tag={Link} className="text-dark" onClick={()=> {logout();}}>Logout</NavLink>
                            </NavItem>}
                        </ul>
                    </Collapse>
                </Container>
            </Navbar>
        </header>
    );
}
