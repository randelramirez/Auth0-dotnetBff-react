/* eslint-disable @typescript-eslint/no-explicit-any */
import { Container } from 'reactstrap';
import { NavMenu } from './NavMenu';

export const Layout = (props:any) => {
    return (
        <div>
            <NavMenu />
            <Container>
                {props.children}
            </Container>
        </div>
    );
}
