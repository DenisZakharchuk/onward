# Quick Start Guide

Get the Inventory System up and running in minutes!

## Step 1: Start the Backend

Open a terminal and run:

```bash
cd backend/InventorySystem.API
dotnet run
```

You should see:
```
Now listening on: http://localhost:5000
Now listening on: https://localhost:5001
```

✅ Backend is running!  
📚 API Documentation: http://localhost:5000/swagger

## Step 2: Start the Frontend

Open a **new terminal** and run:

```bash
cd frontend
npm run dev
```

You should see:
```
VITE v7.3.1  ready in XXX ms
➜  Local:   http://localhost:5173/
```

✅ Frontend is running!  
🌐 Open: http://localhost:5173

## Step 3: Try It Out

### Create a Category
1. Click "Categories" in the navigation
2. Click "Add Category"
3. Enter name: "Electronics"
4. Click "Create"

### Create a Product
1. Click "Products" in the navigation
2. Click "Add Product"
3. Fill in the details:
   - Name: "Laptop"
   - Price: 999.99
   - Category: Select "Electronics"
   - Initial Stock: 10
   - Minimum Stock: 2
4. Click "Create"

### Record a Stock Movement
1. Click "Stock Movements" in the navigation
2. Click "Add Movement"
3. Select your product
4. Choose type: "Out"
5. Quantity: 3
6. Click "Create"
7. Go back to Products - stock should be updated!

## Troubleshooting

### Backend won't start
- Make sure .NET 8 SDK is installed: `dotnet --version`
- Check if port 5000 is already in use
- Try: `dotnet clean` then `dotnet build`

### Frontend won't start
- Make sure Node.js is installed: `node --version`
- Run: `npm install` to install dependencies
- Check if port 5173 is already in use

### "Failed to load" errors in UI
- Make sure the backend is running on http://localhost:5000
- Check browser console for CORS errors
- Verify API URL in `frontend/src/services/api.ts`

### No categories in dropdown
- Create at least one category first before adding products

## API Testing with Swagger

Visit http://localhost:5000/swagger to test API endpoints directly:

1. Expand an endpoint (e.g., POST /api/categories)
2. Click "Try it out"
3. Edit the request body
4. Click "Execute"
5. See the response below

## Development Tips

### Hot Reload
Both backend and frontend support hot reload:
- **Backend**: Changes to C# files trigger automatic recompilation
- **Frontend**: Changes to Vue files update instantly in browser

### Debugging
- **Backend**: Use Visual Studio Code with C# extension or Visual Studio 2022
- **Frontend**: Use browser DevTools (F12)

### Project Structure
See [PROJECT_STRUCTURE.md](PROJECT_STRUCTURE.md) for detailed architecture overview.

## What's Included

✅ Product management (CRUD)  
✅ Category management (CRUD)  
✅ Stock movement tracking  
✅ Low stock indicators  
✅ Clean architecture with abstractions  
✅ In-memory data storage (placeholder)  
✅ RESTful API with Swagger docs  
✅ Responsive Vue.js UI  
✅ TypeScript types  

## What's Next

See [README.md](README.md) for:
- Architecture details
- API endpoints documentation
- Implementation roadmap
- Contributing guidelines

## Need Help?

- Check [README.md](README.md) for detailed documentation
- Review [PROJECT_STRUCTURE.md](PROJECT_STRUCTURE.md) for architecture details
- Examine the code - it's well-commented!

Happy coding! 🚀
