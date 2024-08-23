// import { useEffect } from "react";

export const Home = () => {

    // const loadData = async () => {
    //     const response = await fetch('/api/test');
    //     const json = await response.json();
    //     console.log(json);
    // }

    // useEffect(()=>{
    //     loadData();
    // },[])

    return (
        <div>
            <p>Welcome to your new single-page application, built with:</p>
            <ul>
                <li><a href='https://expressjs.com/'>Express.js</a> and <a href='https://nodejs.org/en'>Node.js</a> for cross-platform server-side code</li>
                <li><a href='https://auth0.com/'>Auth0</a> for user authentication</li>
                <li><a href='https://facebook.github.io/react/'>React</a> for client-side code</li>
                <li><a href='http://getbootstrap.com/'>Bootstrap</a> for layout and styling</li>
            </ul>
            <p>The <code>ClientApp</code> subdirectory is a standard React application based on the <code>create-react-app</code> template. If you open a command prompt in that directory, you can run <code>npm</code> commands such as <code>npm test</code> or <code>npm install</code>.</p>
        </div>
    );
}

export default Home;
