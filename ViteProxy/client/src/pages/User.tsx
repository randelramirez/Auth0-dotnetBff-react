/* eslint-disable @typescript-eslint/no-explicit-any */

import { useAuth } from "../context/useAuth";

// import { useAuth } from "../context/AuthContext";


export const User = () => {

    const { user } = useAuth();

    const renderClaimsTable = function (claims:any) {
        console.log('rendering user table')
        return (
            <table className='table table-striped' aria-labelledby="tabelLabel">
                <thead>
                    <tr>
                        <th>Type</th>
                        <th>Value</th>
                    </tr>
                </thead>
                <tbody>
                    {claims.map((claim:any) =>
                        <tr key={claim.type}>
                            <td>{claim.type}</td>
                            <td>{claim.value}</td>
                        </tr>
                    )}
                </tbody>
            </table>
        );
    }

    return (
        <div>
            <h1 id="tabelLabel" >User claims</h1>
            <p>This component demonstrates fetching user identity claims from the server.</p>
            {renderClaimsTable(user)}
        </div>
    );

}
