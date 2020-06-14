const express = require('express');
const app = express();
const port = 3000;

app.use(express.static('public'))

app.get('/', (req, res) => {
    res.sendFile('./Player.html', { root: __dirname });
});

app.listen(port, () => console.log(`listening on port ${port}!`))